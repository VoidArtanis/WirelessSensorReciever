using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImagioDevice;
using MahApps.Metro.Controls;

namespace LightMapper
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private const int ThermalG1 = 800;
        private const int ThermalG2 = 700;
        private readonly DispatcherTimer _dpt = new DispatcherTimer();
        private readonly Queue<int> _q1 = new Queue<int>(581);
        private readonly Queue<int> _q2 = new Queue<int>(581);
        private   WriteableBitmap _writeableBmp;
        private ThermalResistanceSensor _thermal;
        private ThermalResistanceSensor _thermal2;

        public MainWindow()
        {
            InitializeComponent();
            _dpt.Interval = new TimeSpan(0, 0, 0, 0, 10);
            _dpt.Tick += Dpt_Tick;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            IoSerial.Instance.CloseSerial();
        }

        private void Dpt_Tick(object sender, EventArgs e)
        {
            if (_writeableBmp == null || _writeableBmp.Height == 1)
            {
                _writeableBmp = BitmapFactory.New(581, 442);

            }
            GraphImage.Source = _writeableBmp;
            using (_writeableBmp.GetBitmapContext())
            {
                _writeableBmp.Clear();

                //Sensor 2
                var q2Arr = _q2.ToArray();
                var p2 = new List<int>();
                for (var index = 0; index < q2Arr.Count(); index++)
                {
                    var v = q2Arr[index];
                    p2.Add(index );
                    p2.Add((442 - v - 600));
                }

                //Sensor 1
                var p1 = new List<int>();
                var q1Arr = _q1.ToArray();
                for (var index = 0; index < q1Arr.Count(); index++)
                {
                    if (q1Arr.Length > index)
                    {
                        var v = q1Arr[index];
                        p1.Add(index );
                        p1.Add(442 - (v - 600));
                    }
                }

                try
                {
                    drawGrid();
                    _writeableBmp.DrawPolyline(p1.ToArray(), Colors.Green);
                    _writeableBmp.DrawPolyline(p2.ToArray(), Colors.Red);

                    //update Mean values
                    tbkMeanThermal1.Text = "S1: " +  q1Arr.Average().ToString(CultureInfo.InvariantCulture);
                    tbkMeanThermal2.Text = "S2: " + q2Arr.Average().ToString(CultureInfo.InvariantCulture);
                    _writeableBmp.DrawLine(0, (int) ((int) 442 - (q1Arr.Average() - 600)),581, (int) ((int)442 - (q1Arr.Average() - 600)), Colors.DarkSeaGreen);
                    _writeableBmp.DrawLine(0, (int)((int)442 - (q2Arr.Average() - 600)), 581, (int)((int)442 - (q2Arr.Average() - 600)), Colors.IndianRed);

                    //thermal gate
                    _writeableBmp.DrawLine(0, 442 - (ThermalG1-600), 581, 442 - (ThermalG1 - 600), Colors.DarkOliveGreen);
                    _writeableBmp.DrawLine(0, 442 - ThermalG2 - 600, 581, 442 - ThermalG2    - 600, Colors.DarkRed);


                    //update current values
                    tbkThermalR1.Text = "S1: " + _q1.Peek().ToString(CultureInfo.InvariantCulture);
                    tbkThermalR2.Text = "S2: " + _q2.Peek().ToString(CultureInfo.InvariantCulture);


                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        ///     Sensor 1 read event
        /// </summary>
        /// <param name="reading"></param>
        private void Thermal_SensorValueChanged(int reading)
        {
            _q1.Enqueue(reading);

            if (_q1.Count == 581) _q1.Dequeue();
        }

        /// <summary>
        ///     Sensor 2 read event
        /// </summary>
        /// <param name="reading"></param>
        private void Thermal2OnSensorValueChanged(int reading)
        {
            _q2.Enqueue(reading);
            if (_q2.Count == 581) _q2.Dequeue();
        }

        private async void Connect()
        {
            ConnectionStatus.Text = "Attempting to connect.";
            var connected = await IoSerial.Instance.FindAndConnectAsync();
            ConnectionStatus.Text = (connected) ? "Connected" : "Not Connected";
        }

        private void Instance_ConnectionChanged(bool connected)
        {
            Application.Current.Dispatcher.Invoke(
                () => { ConnectionStatus.Text = (connected) ? "Connected" : "Not Connected"; });

            if (connected)
            {
                _thermal = new ThermalResistanceSensor(ThermalResistanceSensor.ThermalSensors.Sensor2, 1000);
                _thermal.SensorValueChanged += Thermal_SensorValueChanged;
                _thermal2 = new ThermalResistanceSensor(ThermalResistanceSensor.ThermalSensors.Sensor1, 1000);
                _thermal2.SensorValueChanged += Thermal2OnSensorValueChanged;

                Thread.Sleep(1000);
                _thermal.StartContinousReading(3);
                _thermal2.StartContinousReading(3);
                _dpt.Start();
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            IoSerial.Instance.ConnectionChanged += Instance_ConnectionChanged;
            Connect();
        }

        void drawGrid()
        {
            for (int i = 0; i < 581; i+=5)
            {
                _writeableBmp.DrawLine(i,0,i, 442, Color.FromArgb(20,255, 255, 255));
            }
            for (int j = 0; j < 442; j += 5)
            {
                _writeableBmp.DrawLine(0, j, 581, j, Color.FromArgb(20, 255, 255, 255));
            }
 
        }

    }
}