using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimeAppDemo.DataProtocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RimeAppDemo
{
    [ComVisible(true)]
    public class ScriptForWeb
    {
        public string Hello()
        {
            return "Hello World!";
        }

        /// <summary>
        /// 下发消息
        /// </summary>
        /// <param name="devEUI"></param>
        /// <param name="down"></param>
        public void Down(string devEUI, string down)
        {
            Form1.Instance.Down(devEUI, down);
        }

        public string GetConfig()
        {
            return ParseHelper.GetStructConfig().ToString(Formatting.None);
        }

        public string CheckConfig(string config)
        {
            return ParseHelper.CheckStruct(config).ToString(Formatting.None);
        }

        public string SaveConfig(string config)
        {
            return ParseHelper.SaveStructConfig(config).ToString(Formatting.None);
        }

        public void Navigate(string url)
        {
            if (url.StartsWith("@"))
            {
                url = Application.StartupPath + @"/" + Regex.Replace(url, "^@", "");
            }
            Form1.Instance.Navigate(url);
        }
    }
}
