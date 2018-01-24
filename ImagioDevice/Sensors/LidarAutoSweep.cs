using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ImagioDevice
{
    public class LidarAutoSweep
    {
        public delegate void ReadSensor(int position, int distance);

        private Timer timer = null;

        /// <summary>
        ///     This is used to initialize the sensor without the continous reading.
        /// </summary>
        /// <param name="functionIndex"></param>
        public LidarAutoSweep(int functionIndex1,int functionIndex2)
        {
            ReaderFunctionIndex1 = Convert.ToInt32(1 + functionIndex1.ToString());
            ReaderFunctionIndex2 = Convert.ToInt32(1 + functionIndex2.ToString());
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
        public LidarAutoSweep(int functionIndex1,int functionIndex2, long readinterval)
        {
            ReaderFunctionIndex1 = Convert.ToInt32(1 + functionIndex1.ToString());
            ReaderFunctionIndex2 = Convert.ToInt32(1 + functionIndex2.ToString());
            if (IoSerial.Instance.Connected)
            {
                IoSerial.Instance.NewMessageArrived += InstanceOnNewMessageArrived;
            }
            StartContinousReading(readinterval);
        }

        public int CurrentValue { get; set; }
        public int ReaderFunctionIndex1 { get; set; }
        public int ReaderFunctionIndex2 { get; set; }
        public int BufferReadAttempts { get; set; } = 15;
        public event  ReadSensor SensorValueChanged;

        /// <summary>
        ///     Initialize the contnous sensor loop.
        ///     This method must not be used if sensor was initialized with continous mode.
        /// </summary>
        /// <param name="readinterval"></param>
        public void StartContinousReading(long readinterval)
        {
         // IoSerial.Instance.Write(ReaderFunctionIndex2.ToString(), CommandType.Command);
            IoSerial.Instance.Write(ReaderFunctionIndex1.ToString(), CommandType.Command);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SendRead();
        }

        private void SendRead()
        {
            if (IoSerial.Instance.Connected)
            {
                //IoSerial.Instance.Write(ReaderFunctionIndex1.ToString(), CommandType.Analog);
            }
        }
        //
        private Message prevMessage;
        private void InstanceOnNewMessageArrived(Message msg)
        {
            
                
        }
    }
}