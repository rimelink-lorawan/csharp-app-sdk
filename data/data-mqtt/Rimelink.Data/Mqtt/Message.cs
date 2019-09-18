using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rimelink.Data.Mqtt
{
    public class Message
    {
        public Message(string jsonData)
        {
            JSON = JObject.Parse(jsonData);  // json 解析  
            String payload = JSON.Value<String>("data");    // 数据部分，base64 编码
            Data = Convert.FromBase64String(payload);   // 原始数据字节数组
        }

        public Message(JObject json, byte[] data)
        {
            JSON = json;
            Data = data;
        }

        /// <summary>
        /// 消息的 JObject 
        /// </summary>
        public JObject JSON { get; set; }

        /// <summary>
        /// payload 的二进制形式
        /// </summary>
        public byte[] Data { get; set; }

        public override string ToString()
        {
            if (JSON == null) return null;

            return JSON.ToString();
        }
    }
}
