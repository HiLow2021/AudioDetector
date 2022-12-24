using System.Diagnostics;
using System.Drawing.Drawing2D;
using NAudio.Wave;
using My.Audio;
using My.IO;
using My.Security;
using MyAudioDetector = My.Audio.AudioDetector;

namespace AudioDetector.WinForms
{
    public partial class Form1 : Form
    {
        private static readonly int _resolutionPoint = 2;
        private readonly MyAudioDetector _audioDetector = new(new WaveFormat(16000, 1));

        private MyAppSettings _appSettings = new();
        private float _max = 0;
        private float _decibelMax = 0;
        private float[]? _fftResult;

        public Form1()
        {
            InitializeComponent();

            Load += (sender, e) => LoadConfigFile();
            FormClosing += (sender, e) =>
            {
                _audioDetector.Dispose();
                while (_audioDetector.AnalyzingState != AnalyzingState.Stopped)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
            };
            FormClosed += (sender, e) => SaveConfigFile();

            exitToolStripMenuItem.Click += (sender, e) => Application.Exit();
            startToolStripMenuItem.Click += (sender, e) => Start();
            stopToolStripMenuItem.Click += (sender, e) => Stop();
            settingsToolStripMenuItem.Click += (sender, e) =>
            {
                using var form = new Form2(_appSettings);
                form.ShowDialog(this);
            };

            pictureBox1.Paint += (sender, e) =>
            {
                if (_fftResult == null)
                {
                    return;
                }

                var resolution = _fftResult.Length / _resolutionPoint;

                for (int i = 0; i < resolution; i++)
                {
                    var width = e.ClipRectangle.Width;
                    var height = e.ClipRectangle.Height;
                    var resolutionWidth = width / (float)resolution;
                    var resolutionHeight = (1 - CalculateMean(_fftResult, i * _resolutionPoint, _resolutionPoint)) * height;
                    using var brush = new LinearGradientBrush(new PointF(resolutionWidth * i, 0), new PointF(resolutionWidth * i, height), Color.Blue, Color.Aquamarine);

                    e.Graphics.FillRectangle(brush, resolutionWidth * i, height - resolutionHeight, resolutionWidth, resolutionHeight);
                }

                static float CalculateMean(float[] source, int offset, int count)
                {
                    var total = 0.0f;
                    for (int i = 0; i < count; i++)
                    {
                        total += source[offset + i];
                    }

                    return total / count;
                }
            };
            pictureBox2.Paint += (sender, e) =>
            {
                e.Graphics.FillRectangle(Brushes.Aquamarine, 0, 0, _max * e.ClipRectangle.Width, e.ClipRectangle.Height);
            };
            pictureBox3.Paint += (sender, e) =>
            {
                e.Graphics.FillRectangle(Brushes.Aquamarine, 0, 0, _decibelMax * e.ClipRectangle.Width, e.ClipRectangle.Height);
            };
            pictureBox4.Paint += (sender, e) =>
            {
                var brush = _audioDetector.IsDetecting ? Brushes.Red : Brushes.Black;

                e.Graphics.FillRectangle(brush, e.ClipRectangle);
            };
            pictureBox1.Resize += (sender, e) => pictureBox1.Refresh();
            pictureBox2.Resize += (sender, e) => pictureBox2.Refresh();
            pictureBox3.Resize += (sender, e) => pictureBox3.Refresh();

            button1.Click += (sender, e) =>
            {
                try
                {
                    if (_audioDetector.AnalyzingState == AnalyzingState.Stopped)
                    {
                        Start();
                    }
                    else if (_audioDetector.AnalyzingState == AnalyzingState.Analyzing)
                    {
                        Stop();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    MessageBox.Show(ex.Message);
                }
            };

            _audioDetector.PeakCalculated += (sender, e) =>
            {
                _max = e.Max;
                _decibelMax = e.DecibelMax;

                Invoke(new Action(() =>
                {
                    pictureBox2.Refresh();
                    pictureBox3.Refresh();
                }));
            };
            _audioDetector.FftCalculated += (sender, e) =>
            {
                var result = e.DecibelResult;

                _fftResult = result.Take(result.Length / 2).ToArray();
                Invoke(new Action(() => pictureBox1.Refresh()));
            };
            _audioDetector.DetectionInitiated += (sender, e) =>
            {
                Invoke(new Action(() => pictureBox4.Refresh()));
            };
            _audioDetector.DetectionFinished += (sender, e) =>
            {
                Invoke(new Action(() => pictureBox4.Refresh()));
            };
        }

        private void Start()
        {
            _audioDetector.ThresholdStart = _appSettings.ThresholdStart;
            _audioDetector.ThresholdEnd = _appSettings.ThresholdEnd;
            _audioDetector.DetectionSeconds = _appSettings.DetectionSeconds;
            _audioDetector.SilenceSeconds = _appSettings.SilenceSeconds;
            _audioDetector.Start(_appSettings.InputDeviceNumber);

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            button1.Text = "ストップ";
        }

        private void Stop()
        {
            _audioDetector.Stop();

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            pictureBox4.Refresh();
            button1.Text = "スタート";
        }

        private void LoadConfigFile()
        {
            if (FileAdvanced.Exists(MyAppSettings.ConfigPath))
            {
                var bs = FileAdvanced.LoadFromBinaryFile<byte[]>(MyAppSettings.ConfigPath);

                _appSettings = Cryptography.Decrypt<MyAppSettings>(bs);
            }
        }

        private void SaveConfigFile()
        {
            FileAdvanced.SaveToBinaryFile(MyAppSettings.ConfigPath, Cryptography.Encrypt(_appSettings));
        }
    }
}