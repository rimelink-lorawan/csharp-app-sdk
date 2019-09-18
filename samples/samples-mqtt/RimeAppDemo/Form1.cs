using Rimelink.Data.Mqtt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RimeAppDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Rime LoRa Data 客户端
        private Client client = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. 从配置文件中获取相关连接授权信息
            // 服务器地址
            string broker = ConfigurationManager.AppSettings["broker"];
            // 如果服务器地址是锐米发货云，appId 为 应用集成 中的 MQTT 用户名
            // 如果为私有云或本地部署，appId 为 Applications 中 Device 节点所挂载的应用的 ID
            string appId = ConfigurationManager.AppSettings["appId"];
            // 如果并未开启 mosquitto 的授权机制，则保持为空
            string appSecretKey = ConfigurationManager.AppSettings["appSecretKey"];

            // 2. 实例化客户端
            client = new Client(broker, appId, appSecretKey); 

            // 3. 相关通知响应
            // 3.1 上行消息到达处理交给 DataReceived 方法
            client.OnMessage(DataReceived);

            // 3.2 连接成功时
            client.OnConnected(() =>
            {
                Invoke(new MethodInvoker(delegate
                {
                    lblState.ForeColor = Color.Green;
                    lblState.Text = "连接成功";
                }));
            });

            // 3.3 连接失败、连接断开、异常断开时
            client.OnError((c, message) =>
            {
                Invoke(new MethodInvoker(delegate
                {
                    lblState.ForeColor = Color.Red;
                    lblState.Text = message;
                }));
            });

            // 4. 启动客户端
            client.Start();
            lblState.Text = "连接中...";
        }
         
        // 设备 devEUI 对应的 UI 组件
        private Dictionary<string, DevicePanel> _DevicePanelDict = new Dictionary<string, DevicePanel>(); 
        /// <summary>
        /// 上行消息到达处理
        /// 本实例中，
        /// </summary>
        /// <param name="devEUI"></param>
        /// <param name="message"></param>
        private void DataReceived(string devEUI, Rimelink.Data.Mqtt.Message message)
        {
            if (client == null) return;

            // 上行的已解密还原的节点发送上来的数据
            byte[] data = message.Data;

            try
            {
                Log(String.Format("接收 {0} 数据\r\n{1}", devEUI, BitConverter.ToString(data)).Replace('-', ' '));
                if (data != null)
                {
                    string devName = message.JSON.Value<string>("deviceName");
                    Invoke(new MethodInvoker(delegate
                    {
                        if (!_DevicePanelDict.ContainsKey(devEUI))
                        {
                            var devicePanel = new DevicePanel(devEUI, devName);
                            _DevicePanelDict[devEUI] = devicePanel;
                            devicePanel.MarqueeClick(btnSend_Click);

                            layoutPanel.Controls.Add(devicePanel);
                        }

                        _DevicePanelDict[devEUI].DataReceived(data);
                    }));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("处理数据异常：" + e.Message);
            }
        }

        private bool _AllowLoging = true;
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="info"></param>
        private void Log(string info)
        {
            if (!_AllowLoging) return;

            Invoke(new MethodInvoker(delegate
            {
                txtLog.AppendText(String.Format("{0} {1}\r\n\r\n", DateTime.Now.ToString("MM-dd HH:mm:ss"), info));
            }));
        }

        /// <summary>
        /// 跑马灯：向节点发送数据 led 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        { 
            if (!client.IsConnected)
            { 
                return;
            }

            string devEUI = ((Button)sender).Tag as string;
            if (!String.IsNullOrEmpty(devEUI) && client.IsConnected)
            {
                var data = Encoding.ASCII.GetBytes("led");
                client.Send(devEUI, data, 10);
                Log(String.Format("发送 {0} 数据\r\n(led) {1}", devEUI, BitConverter.ToString(data)).Replace('-', ' '));
            } 
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.End();
            client = null; 
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnSetLog_Click(object sender, EventArgs e)
        {
            _AllowLoging = !_AllowLoging;

            btnSetLog.Text = _AllowLoging ? "暂停日志" : "显示日志";
        } 
    }
}
