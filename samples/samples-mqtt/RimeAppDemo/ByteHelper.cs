using System;
using System.Collections.Generic;
using System.Text;

namespace RimeAppDemo
{
public class ByteHelper
{
    public static string BytesToHexString(byte[] bytes)
    {
        return BytesToHexString(bytes, " ");
    }

    public static string BytesToHexString(byte[] bytes, string span)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * (2 + span.Length));
        bool isFirst = true;
        foreach (byte b in bytes)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                sb.Append(span);
            }
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').ToUpper());
        }
        return sb.ToString();
    }

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

    /// <summary>
    /// 字节数组转整型
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static int GetIntValue(byte[] bytes)
    {
        return Convert.ToInt32(GetLongValue(bytes));
    }

    public static uint GetUnsignedIntValue(byte[] bytes)
    {
        return Convert.ToUInt32(GetUnsingedLongValue(bytes));
    }

    /// <summary>
    /// 将整型按小端转换成指定长度的byte数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static byte[] IntToBytes(int value, int len)
    {
        byte[] bytes = new byte[len];
        string strHexValue = Convert.ToString(value, 16).PadLeft(len * 2, '0');
        for (int index = 0; index < len; index++)
        {
            bytes[index] = Convert.ToByte(strHexValue.Substring(strHexValue.Length - (index + 1) * 2, 2), 16);
        }
        return bytes;
    }

    public static byte[] SubBytes(byte[] bytes, int startIndex, int len)
    {
        if (startIndex + len > bytes.Length) len = bytes.Length - startIndex;

        byte[] subBytes = new byte[len];
        Array.Copy(bytes, startIndex, subBytes, 0, len);
        return subBytes;
    }

    # region 以长整型支持最多 8 个字节的整数转换

    /// <summary>
    /// 非 1，2，4，8 字节的其它字节转换
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static long GetIrregularLongValue(byte[] bytes)
    {
        long value = 0; // 循环读取每个字节通过移位运算完成long的4个字节拼装
        for (int count = 0; count < bytes.Length; ++count)
        {

            int shift = count << 3;
            value |= ((long)0xff << shift) & ((long)bytes[count] << shift);
        }
        return value;
    }

    /// <summary>
    /// 只考虑小端（有符号）
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static long GetLongValue(byte[] bytes)
    {
        if (bytes.Length == 1)
        {
            return (long)((sbyte)bytes[0]);
        }
        else if (bytes.Length == 2)
        {
            return (long)BitConverter.ToInt16(bytes, 0);
        }
        else if (bytes.Length == 4)
        {
            return (long)BitConverter.ToInt32(bytes, 0);
        }
        else if (bytes.Length == 8)
        {
            return (long)BitConverter.ToInt64(bytes, 0);
        }
        return GetIrregularLongValue(bytes);
    }

    /// <summary>
    /// 只考虑小端（无符号）
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ulong GetUnsingedLongValue(byte[] bytes)
    {
        if (bytes.Length == 1)
        {
            return (ulong)bytes[0];
        }
        else if (bytes.Length == 2)
        {
            return (ulong)BitConverter.ToUInt16(bytes, 0);
        }
        else if (bytes.Length == 4)
        {
            return (ulong)BitConverter.ToUInt32(bytes, 0);
        }
        else if (bytes.Length == 8)
        {
            return (ulong)BitConverter.ToUInt64(bytes, 0);
        }
        return (ulong)GetIrregularLongValue(bytes);
    }

    #endregion
}
}
