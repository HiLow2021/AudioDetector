using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDetector.WinForms
{
    [Serializable]
    public class MyAppSettings
    {
        public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string ConfigPath { get; } = BaseDirectory + "config.dat";
        public int InputDeviceNumber { get; set; } = 0;
        public float ThresholdStart { get; set; } = 0.2f;
        public float ThresholdEnd { get; set; } = 0.2f;
        public float DetectionSeconds { get; set; } = 0.0f;
        public float SilenceSeconds { get; set; } = 1.0f;
    }
}
