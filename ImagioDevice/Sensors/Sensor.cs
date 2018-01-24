using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ImagioDevice
{
    public class Sensor
    {
        public delegate void ReadSensor(int reading);

        private Timer timer = null;

        /// <summary>
        ///     This is used to initialize the sensor without the continous reading.
        /// </summary>
        /// <param name="functionIndex"></param>
        public Sensor(int functionIndex)
        {
            ReaderFunctionIndex = Convert.ToInt32(1 + functionIndex.ToString());
            if (IoSerial.Instance.Connected)
            {
                IoSerial.Instance.NewMessageArrived += InstanceOnNewMessageArrived;
            }
        }

        /// <summary>
        ///     This is used to initialize the sensor for continous reading
        /// </summary>
        /// <param name="functionIndex"></param>
        /// <param name="readinterval"></param>
        public Sensor(int functionIndex, long readinterval)
        {
            ReaderFunctionIndex = Convert.ToInt32(1 + functionIndex.ToString());
            if (IoSerial.Instance.Connected)
            {
                IoSerial.Instance.NewMessageArrived += InstanceOnNewMessageArrived;
            }
            StartContinousReading(readinterval);
        }

        public int CurrentValue { get; set; }
        public int ReaderFunctionIndex { get; set; }
        public int BufferReadAttempts { get; set; } = 15;
        public event ReadSensor SensorValueChanged;

        /// <summary>
        ///     Initialize the contnous sensor loop.
        ///     This method must not be used if sensor was initialized with continous mode.
        /// </summary>
        /// <param name="readinterval"></param>
        public void StartContinousReading(long readinterval)
        {
            //            if (timer == null)
            //            {
            //                timer = new Timer(readinterval);
            //
            //                // Hook up the Elapsed event for the timer.
            //                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //
            //                timer.Enabled = true;
            //            }
            IoSerial.Instance.Write(ReaderFunctionIndex.ToString(), CommandType.Command);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SendRead();
        }

        private void SendRead()
        {
            if (IoSerial.Instance.Connected)
            {
                //IoSerial.Instance.Write(ReaderFunctionIndex.ToString(), CommandType.Analog);
            }
        }

        public Task<int> SensorReadAsync()
        {
            return Task.Factory.StartNew(() => SensorRead());
        }

        /// <summary>
        ///     Delays 15ms and tries to get response from device. Maximum attempts can be specified
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        private int SensorRead()
        {
            if (IoSerial.Instance.Connected)
            {
                var found = false;
                var i = 0;
                // we dont know when the response will come, so lets just go through the messages till we hit gold.
                while (!found)
                {
                    var val = IoSerial.Instance.ReadBuffer(ReaderFunctionIndex.ToString(), CommandType.Analog);
                    if (val != null)
                        return Convert.ToInt32(val);

                    //quit after max attempts
                    if (i++ > BufferReadAttempts) break;

                    //because duh..
                    //give the device some time.
                    Thread.Sleep(15);
                }
            }
            return -1;
        }

        private void InstanceOnNewMessageArrived(Message msg)
        {
            if (SensorValueChanged != null)
                if (msg != null && msg.Command != String.Empty)
                {
                    try
                    {
                        if (msg.Response.FunctionIndex == ReaderFunctionIndex.ToString())
                            //update the events if the this is the right sensor
                        {
                            CurrentValue = Convert.ToInt32(Response.GetResponse(msg).Value);
                            SensorValueChanged?.Invoke(Convert.ToInt32(Response.GetResponse(msg).Value));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
        }
    }
}