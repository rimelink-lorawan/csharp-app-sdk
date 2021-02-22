using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimeAppDemo.DataProtocal
{
    public class ParseHelper
    {
        private static JObject _StructConfig;

        public static JObject GetStructConfig()
        {
            if (_StructConfig == null)
            {
                string json = File.ReadAllText("config.json");
                _StructConfig = JObject.Parse(json);
            }

            return _StructConfig;
        }

        public static JObject CheckStruct(string structConfigStr)
        { 
            try
            {
                return CheckStruct(JObject.Parse(structConfigStr));
            }
            catch (Exception e)
            {
                JObject result = new JObject();
                result["err_code"] = 111;
                result["err_msg"] = e.Message;
                return result;
            }
        }

        /// <summary>
        /// 规则检查：
        /// 长度：大于 0
        /// 大小端：大、小
        /// 类型：整型、无符号整型、浮点型、字符型、其它
        /// 整型：长度 1,2,4,8 
        /// 无符号整型：长度 1 - 8 
        /// 浮点型：4 位
        /// 双精度：8 位
        /// </summary>
        /// <param name="structConfig"></param>
        /// <returns></returns>
        public static JObject CheckStruct(JObject structConfig)
        {
            JObject result = new JObject();
            try
            {
                JArray items = structConfig.Value<JArray>("数据结构");
                int totallen = 0;
                foreach (JObject item in items)
                {

                    int len = item.Value<int>("长度");
                    string label = item.Value<string>("标签");
                    string type = item.Value<string>("类型");
                    string begin = item.Value<string>("大小端");
                    string unit = item.Value<string>("单位");

                    if (len <= 0) throw new Exception("长度需要大于 0");
                    if (!new string[] { "大", "小" }.Contains(begin)) throw new Exception("大小端设置不正确");
                    if (!new string[] { "整型", "无符号整型", "浮点型", "双精度", "字符型", "其它" }.Contains(type)) throw new Exception("类型设置不正确");

                    // 规则检查
                    if ("整型".Equals(type))
                    {
                        if (!new int[] { 1, 2, 4, 8 }.Contains(len)) throw new Exception("整型长度仅可为 1，2，4，8");
                    }
                    else if ("无符号整型".Equals(type))
                    {
                        if (len > 8) throw new Exception("无符号整型长度不能超过 8");
                    }
                    else if ("浮点型".Equals(type))
                    {
                        if (len != 4) throw new Exception("浮点型长度应为 4");
                    }
                    else if ("双精度".Equals(type))
                    {
                        if (len != 8) throw new Exception("双精度长度应为 8");
                    }
                    totallen += len;
                }

                result["err_code"] = 200;
                result["err_msg"] = "success";
                result["total_length"] = totallen;
                return result;
            } 
            catch (Exception e)
            {
                result["err_code"] = 111;
                result["err_msg"] = e.Message;
                return result;
            }
        }

        public static JObject SaveStructConfig(string structConfigStr)
        {
            try
            {
                JObject structConfig = JObject.Parse(structConfigStr);
                return SaveStructConfig(structConfig);
            }
            catch (Exception e)
            {
                JObject result = new JObject();
                result["err_code"] = 111;
                result["err_msg"] = e.Message;
                return result;
            }
        }

        public static JObject SaveStructConfig(JObject structConfig)
        {
            var checkResult = CheckStruct(structConfig);
            if (checkResult.Value<int>("err_code") == 200)
            {
                try
                {
                    File.WriteAllText("config.json", structConfig.ToString());
                }
                catch(Exception e)
                {
                    JObject result = new JObject();
                    result["err_code"] = 111;
                    result["err_msg"] = e.Message;
                    return result;
                }
                _StructConfig = structConfig; 
            }
            return checkResult;
        }

        /// <summary>
        /// 解析数据
        /// 根据配置："长度":2, "标签":"距离","类型":"整型","大小端":"大","单位":"m"
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static JObject ParseData(byte[] data)
        {
            JObject result = new JObject();
            JArray values = new JArray();
            JArray items = GetStructConfig().Value<JArray>("数据结构");
            int startIndex = 0;
            foreach (JObject item in items)
            {
                if (startIndex >= data.Length) return null;

                JObject valueItem = new JObject(); 

                int len = item.Value<int>("长度");
                string label = item.Value<string>("标签");
                string type = item.Value<string>("类型");
                string begin = item.Value<string>("大小端");
                string unit = item.Value<string>("单位");

                valueItem["label"] = label;


                byte[] bs = ByteHelper.SubBytes(data, startIndex, len);
                if ("整型".Equals(type))
                {
                    if ("大".Equals(begin))
                    {
                        Array.Reverse(bs);
                    }
                    valueItem["value"] = ByteHelper.GetLongValue(bs);
                }
                else if ("无符号整型".Equals(type))
                {
                    if ("大".Equals(begin))
                    {
                        Array.Reverse(bs);
                    }
                    valueItem["value"] = ByteHelper.GetUnsingedLongValue(bs);
                }
                else if ("浮点型".Equals(type))
                {
                    if ("大".Equals(begin))
                    {
                        Array.Reverse(bs);
                    }
                    valueItem["value"] = Math.Round(BitConverter.ToSingle(bs, 0), 4);
                }
                else if ("双精度".Equals(type))
                {
                    if ("大".Equals(begin))
                    {
                        Array.Reverse(bs);
                    }
                    valueItem["value"] = Math.Round(BitConverter.ToDouble(bs, 0), 4);
                }
                else if ("字符型".Equals(type))
                {
                    valueItem["value"] = Encoding.Default.GetString(bs);
                }
                else
                {
                    valueItem["value"] = ByteHelper.BytesToHexString(bs);
                }

                valueItem["label"] = label;
                valueItem["unit"] = unit;

                values.Add(valueItem);
                startIndex += len;
            }
            result["values"] = values;
            return result;
        }
    }
}
