using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Rimelink.Data.Mqtt
{
    public class Client
    {
        public delegate void MessageDelegate(string devEUI, Message message);
        public delegate void ErrorDelegate(Client client, string errMsg);
        public delegate void NotifyDelegate();

        #region 私有部分
        private MessageDelegate _MessageDelegate;
        private ErrorDelegate _ErrorDelegate;
        private NotifyDelegate _ConnectedDelegate;

        // MQTT 服务端及账户信息
        private string _MqttServer;
        private string _MqttAppId;
        private string _MqttUsername;
        private string _MqttUserpass;
        private string _MqttClientId;

        // 客户端实例 
        private MqttClient _MqttClient;

        // 尝试连接的次数记录
        private int _TryCount = 0;
        private bool _IsTrying = false;

        // 表示进入关闭阶段
        private bool _ToClose = false; 

        // 自动重连主体
        private void _TryContinueConnect()
        {
            if (IsConnected) return;

            if (_IsTrying) return;

            _IsTrying = true;
            Thread retryThread = new Thread(new ThreadStart(delegate
            {
                while (_MqttClient == null || !_MqttClient.IsConnected)
                {
                    if (_ToClose) break; 

                    try
                    {
                        _TryCount++;
                        Debug.WriteLine("try :" + _TryCount);
                        _Connect(); 
                    }
                    catch (Exception ce)
                    {
                        Debug.WriteLine("re connect exception:" + ce.Message);
                    }

                    // 如果还没连接不符合结束条件则睡2秒
                    if (_MqttClient == null || !_MqttClient.IsConnected)
                    {
                        Thread.Sleep(3000);
                    }
                }

                _IsTrying = false;
            }));

            retryThread.IsBackground = true;
            
            retryThread.Start();
        }

        private bool _BuildClient()
        {
            if (_MqttClient == null)
            {
                _MqttClient = new MqttClient(_MqttServer);

                // 消息到达事件绑定
                _MqttClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                // 连接断开事件绑定
                _MqttClient.ConnectionClosed += (sender, e) =>
                {
                    if (!_ToClose)
                    {
                        if (_ErrorDelegate != null) _ErrorDelegate(this, "连接断开！");

                        // 尝试重连
                        _TryContinueConnect();
                    }
                };
            }

            return _MqttClient != null;
        }

        // 发起一次连接，连接成功则订阅相关主题 
        private void _Connect()
        {
            try
            {
                if (_BuildClient())
                {
                    if (String.IsNullOrEmpty(_MqttUserpass)) 
                        _MqttClient.Connect(_MqttClientId); 
                    else 
                        _MqttClient.Connect(_MqttClientId, _MqttUsername, _MqttUserpass); 
                }
            }
            catch (Exception e)
            {
                if (_ErrorDelegate != null) _ErrorDelegate(this, e.Message);
                return;
            }

            if (_MqttClient.IsConnected)
            {
                if (_ConnectedDelegate != null) _ConnectedDelegate();

                _MqttClient.Subscribe(
                   new string[] {
                        "application/" + _MqttAppId + "/node/+/rx",
                        "application/" + _MqttAppId + "/device/+/rx",
                        "application/" + _MqttAppId + "/device/+/event/up"
                   },
                   new byte[] {
                        MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                        MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                        MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE
                   });
            }
            else
            {
                if (_ErrorDelegate != null) _ErrorDelegate(this, "连接失败！");
            }
        } 

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string msg = System.Text.Encoding.Default.GetString(e.Message);

            bool isUpEvent = e.Topic.EndsWith("/event/up");
            if (e.Topic.EndsWith("rx") || isUpEvent)   // 上行数据消息接收
            {
                version = isUpEvent ? 3 : 0;
                // 收到数据
                JObject json = JObject.Parse(msg);  // json 解析 
                String devEUI = json.Value<String>("devEUI");   // 数据来自终端的devEUI
                String payload = json.Value<String>("data");    // 数据部分，base64 编码
                byte[] data = null;
                if (!String.IsNullOrEmpty(payload))
                {
                    data = Convert.FromBase64String(payload);   // 原始数据字节数组
                }

                Message message = new Message(json, data);
                if (_MessageDelegate != null)
                {
                    _MessageDelegate(devEUI, message);
                }
            }
        }

        #endregion

        public bool IsConnected { get {
            if (_MqttClient != null) 
                return _MqttClient.IsConnected;
            return false;
        } }

        public Client(string broker, string appId, string appAccessKey)
            : this(broker, appId, appId, appAccessKey)
        {
        }

        public Client(string broker, string username, string appId, string appAccessKey)
        {
            _MqttServer = broker;
            _MqttAppId = appId;
            _MqttUsername = username;
            _MqttUserpass = appAccessKey;
            _MqttClientId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 启动客户端
        /// </summary>
        /// <returns></returns>
        public Client Start()
        {
            _ToClose = false;
            if (!IsConnected)
            {
                _TryContinueConnect();
            }
            return this;
        }

        /// <summary>
        /// 停止客户端
        /// </summary>
        /// <returns></returns>
        public Client End()
        {
            _ToClose = true;
            if (_MqttClient !=null && _MqttClient.IsConnected)
            {
                _MqttClient.Disconnect();
            }
            return this;
        }

        /// <summary>
        /// 上行消息到达处理
        /// </summary>
        /// <param name="onmessage"></param>
        public void OnMessage(MessageDelegate onmessage)
        {
            _MessageDelegate = onmessage;
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="onconnected"></param>
        public void OnConnected(NotifyDelegate onconnected)
        {
            _ConnectedDelegate = onconnected;
        }

        /// <summary>
        /// 连接断开事件
        /// </summary>
        /// <param name="onerror"></param>
        public void OnError(ErrorDelegate onerror)
        {
            _ErrorDelegate = onerror;
        }

        public void Send(string devEUI, byte[] data, int port)
        {
            Send(devEUI, data, port, false);
        }

        private int version = 0;

        /// <summary>
        /// 发送下行消息
        /// </summary>
        /// <param name="devEUI"></param>
        /// <param name="data"></param>
        /// <param name="port"></param>
        /// <param name="confirmed"></param>
        public void Send(string devEUI, byte[] data, int port, bool confirmed)
        {
            if (IsConnected)
            {
                JObject downData = new JObject();
                downData["dev_eui"] = devEUI;
                downData["confirmed"] = confirmed;
                downData["fPort"] = port;
                downData["data"] = Convert.ToBase64String(data);
                String downJson = downData.ToString(Formatting.None);
                if (version < 3)
                {
                    _MqttClient.Publish("application/" + _MqttAppId + "/node/" + devEUI + "/tx", Encoding.UTF8.GetBytes(downJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                    _MqttClient.Publish("application/" + _MqttAppId + "/device/" + devEUI + "/tx", Encoding.UTF8.GetBytes(downJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                }
                _MqttClient.Publish("application/" + _MqttAppId + "/device/" + devEUI + "/command/down", Encoding.UTF8.GetBytes(downJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
        }
    }
    
}
