using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace AudioDetector.WinForms
{
    public partial class Form2 : Form
    {
        private MyAppSettings? _appSettings;

        public Form2(MyAppSettings appSettings)
        {
            InitializeComponent();

            Load += (sender, e) =>
            {
                _appSettings = appSettings;

                var i = 0;
                foreach (var item in new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    comboBox1.Items.Add(item.FriendlyName);
                    i++;
                }
                if (_appSettings.InputDeviceNumber < i)
                {
                    comboBox1.SelectedIndex = _appSettings.InputDeviceNumber;
                }
                else if (i > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }

                numericUpDown1.Value = (decimal)_appSettings.ThresholdStart;
                numericUpDown2.Value = (decimal)_appSettings.ThresholdEnd;
                numericUpDown3.Value = (decimal)_appSettings.DetectionSeconds;
                numericUpDown4.Value = (decimal)_appSettings.SilenceSeconds;
            };

            button1.Click += (sender, e) =>
            {
                if (_appSettings == null)
                {
                    return;
                }

                _appSettings.InputDeviceNumber = comboBox1.SelectedIndex;
                _appSettings.ThresholdStart = (float)numericUpDown1.Value;
                _appSettings.ThresholdEnd = (float)numericUpDown2.Value;
                _appSettings.DetectionSeconds = (float)numericUpDown3.Value;
                _appSettings.SilenceSeconds = (float)numericUpDown4.Value;
            };
        }
    }
}
