using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimeAppDemo.DataProtocal
{
    /// <summary>
    /// 温湿度解析
    /// 温湿度数据包括12字节为：温度（4字节）、湿度（4字节）及露点（4字节），均为大端模式的浮点数。
    /// </summary>
    public class TemperatureAndHumidity
    {
        public float Temperature;  // 温度
        public float Humidity;    // 湿度
        public float DewPoint;    // 露点


        /// <summary>
        /// 解析温湿度，参数为 16 进制字节序列文本
        /// 如：41 b9 5c 2a 42 4c 8e 2f 41 48 36 b8 00
        /// </summary>
        /// <param name="hexStr">以 16 进制表示的字节序列，每字节可以以空格隔开也可以没有空格</param>
        /// <returns></returns>
        public static TemperatureAndHumidity Parse(string hexStr)
        {
            try
            {
                byte[] bs = HexStringToBytes(hexStr);
                return Parse(bs);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析温湿度
        /// </summary>
        /// <param name="bs">12个字节的数组</param>
        /// <returns></returns>
        public static TemperatureAndHumidity Parse(byte[] bs)
        {
            try
            {
                byte[] bt = FormatBigFlotBytes(bs, 0);

                float t = BitConverter.ToSingle(bt, 0);  // 前4字节温度
                float h = 0f;
                if (bs.Length >= 8)
                {
                    byte[] bh = FormatBigFlotBytes(bs, 4);
                    h = BitConverter.ToSingle(bh, 0);      // 湿度
                }
                float d = 0f;
                if (bs.Length >= 12)
                {
                    byte[] bd = FormatBigFlotBytes(bs, 8);
                    d = BitConverter.ToSingle(bd, 0);
                }
                TemperatureAndHumidity tah = new TemperatureAndHumidity();
                tah.Temperature = t;
                tah.Humidity = h;
                tah.DewPoint = d;

                return tah;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将大端模式的反转过来
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static byte[] FormatBigFlotBytes(byte[] bs, int start)
        {
            byte[] bsr = new byte[4];
            bsr[3] = bs[start];
            bsr[2] = bs[start + 1];
            bsr[1] = bs[start + 2];
            bsr[0] = bs[start + 3];
            return bsr;
        }

        /// <summary>
        /// 16 进制字节序列转换为字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Replace(" ", ""), 16);
            return returnBytes;
        }
    }
}
