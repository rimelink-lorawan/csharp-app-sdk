using RimeAppDemo.DataProtocal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RimeAppDemo
{
    public class DevicePanel  : Panel
    {
        private string _DevEUI;
        private string _DevName;

        private Button _ButtonSend;
        private Label _TopLabel;
        private Label _MainLabel;
        private Label _BottomLabel;

        public string DevEUI
        {
            get { return _DevEUI; }
        }

        public DevicePanel(string devEUI, string devName)
        {
            this.Width = 210;
            this.Height = 110;
            this.Margin = new System.Windows.Forms.Padding(10);

            _DevEUI = devEUI;
            _DevName = devName;

            _ButtonSend = new Button();
            _ButtonSend.Text = "跑马灯";
            _ButtonSend.Top = Height - _ButtonSend.Height - 5;
            _ButtonSend.Left = Width - _ButtonSend.Width - 5;
            _ButtonSend.FlatStyle = FlatStyle.Flat;
            _ButtonSend.Tag = devEUI;
            this.Controls.Add(_ButtonSend);

            _TopLabel = new Label();
            _MainLabel = new Label();
            _BottomLabel = new Label();

            _TopLabel.Padding = new System.Windows.Forms.Padding(5);
            _TopLabel.AutoSize = true;
            _TopLabel.Text = String.Format("{0}\r\n设备名称：{1}", devEUI, devName);

            _MainLabel.Top = 35;
            _MainLabel.AutoSize = true;
            _MainLabel.Font = new System.Drawing.Font("Arial", 22);
            _MainLabel.ForeColor = Color.Pink;
            _MainLabel.Padding = new System.Windows.Forms.Padding(1);

            _BottomLabel.Top = 75;
            _BottomLabel.AutoSize = true;
            _BottomLabel.Padding = new System.Windows.Forms.Padding(5);

            this.Controls.Add(_TopLabel);
            this.Controls.Add(_MainLabel);
            this.Controls.Add(_BottomLabel);

            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }

        public void MarqueeClick(EventHandler onclick)
        {
            _ButtonSend.Click += onclick;
        }

        public void DataReceived(byte[] data)
        {
            var temperature = TemperatureAndHumidity.Parse(data);
            if (temperature != null)
            { 
                _MainLabel.Text = temperature.Temperature.ToString() + "℃";
                _BottomLabel.Text = String.Format("湿度：{0}%\r\n{1}", Math.Round(temperature.Humidity), DateTime.Now.ToString("MM-dd HH:mm:ss"));
            }
        }
    }
}
