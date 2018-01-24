using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImagioDevice
{
    public class IoSerial
    {
        public delegate void ConnectionChangedEvent(bool connected);

        public delegate void MessageArrived(Message msg);

        public SerialPort Port { get; set; }
        public int BaudRate { get; set; } = 9600;
        public String PortName { get; set; }
        public int MessagePoolBufferSize { get; set; } = 100;

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                // notify if the connection changes.
                ConnectionChanged?.Invoke(_connected);
            }
        }

        public event ConnectionChangedEvent ConnectionChanged;
        public event MessageArrived NewMessageArrived;

        #region Connect

        public List<String> GetPortNames()
        {
            if (SerialPort.GetPortNames().Count() >= 0)
            {
                return SerialPort.GetPortNames().ToList();
            }

            return new List<string>();
        }

        public void EchoMessage(string text)
        {
            Write(text, CommandType.Echo);
        }

        public Boolean FindAndConnect()
        {
            if (Connected) return true;
            lock (this) // for thread safety
            {
                var portList = GetPortNames();
                foreach (var port in portList)
                {
                    var myPort = new SerialPort(port, BaudRate);
                    try
                    {
                        myPort.Open();
                        Thread.Sleep(50); //give it some time
                        myPort.DiscardInBuffer();
                        myPort.WriteLine(string.Format("!{0}:00;", (int) CommandType.HandShake));
                        Console.WriteLine(string.Format("!{0}:00;", (int) CommandType.HandShake));

                        var cycles = 0;
                        while (cycles++ < 15)
                        {
                            if (myPort.BytesToRead > 0)
                            {
                                var ret = myPort.ReadLine();
                                if (ret != String.Empty)
                                {
                                    var message = Message.ParseMessage(ret);
                                    if (message != null)
                                    {
                                        Console.WriteLine(message.Command);
                                        if (message.Type == CommandType.HandShake) //device also returned the handshake
                                        {
                                            myPort.Close();
                                            myPort.Dispose();
                                            _connected = true;
                                            BeginSerial(BaudRate, port);
                                            Console.WriteLine("Connected to" + port);
                                            break;
                                        }
                                    }
                                }
                            }
                            Thread.Sleep(30); //give it some time
                        }
                        if(_connected) break;
                    }
                    catch (UnauthorizedAccessException accessException)
                    {
                        Console.WriteLine(accessException.Message);
                    }
                    finally
                    {
                        myPort.Close();;
                    }
                }
            }
            return Connected;
        }

        public Task<bool> FindAndConnectAsync()
        {
            return Task.Factory.StartNew(() => FindAndConnect());
        }

        #endregion Connect

        #region Communication

        public bool Write(String command, CommandType type)
        {
            if (Connected)
            {
                Port?.WriteLine("!" + (int) type + ":" + command + ";");
                Console.WriteLine("!" + (int)type + ":" + command + ";");
                return true;
            }
            return false;
        }

        public bool Write(int command, CommandType type)
        {
            if (Connected)
            {
                Port?.WriteLine("!" + (int) type + ":" + command + ";");
                Console.WriteLine("!" + (int)type + ":" + command + ";");
                return true;
            }
            return false;
        }

        /// <summary>
        ///     No delays, tries to get response from current buffer
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string ReadBuffer(String command, CommandType type)
        {
            if (Connected)
            {
                // we dont know when the response will come, so lets just go through the messages till we hit gold.
                //iterate the buffer
                foreach (var msg in messageBuffer)
                {
                    if (msg.Command.Split('=')[0] == command && msg.Type == type)
                    {
                        return msg.Response.Value;
                    }
                }
            }
            return null;
        }

        //messages buffer
        private static readonly Queue<Message> messageBuffer = new Queue<Message>();
        private static readonly Queue<byte> MessageQueue = new Queue<byte>();

        private bool _responseComplete = false;
        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            try
            {
                if (Port.IsOpen)
                {
                    var str = Port.ReadLine();

                    //save messages in a buffer if it is needed to be read within a certain time.
                    lock (messageBuffer)
                    {
                        messageBuffer.Enqueue(Message.ParseMessage(str));

                    }

                    var msg = Message.ParseMessage(str);
                    NewMessageArrived?.Invoke(msg);
                }
                // Port.DiscardInBuffer();
            }
            catch (Exception ex)
            {

               Console.WriteLine("DataRecieved Error:{0}", ex.Message);
            }
        }

        #endregion Communication

        #region Innitializer

        public bool BeginSerial(int baud, string portname)
        {
            BaudRate = baud;
            PortName = portname;
            Port = new SerialPort(portname, baud);
            Port.DataReceived += PortOnDataReceived;
            try
            {
                Port.DtrEnable = true;
                Port.Parity = Parity.None;
                Port.NewLine = ";";
                Port.Open();
                     
                Connected = true;
                Thread thread =new Thread(parseMessages);
                thread.Start();
            }
            catch (Exception exception)
            {
                Port.Dispose();
                Connected = false;
            }
            return Connected;
        }

        private void parseMessages(object o)
        {
       
        }


        public bool BeginSerial()
        {
            Port = new SerialPort(PortName, BaudRate);
            Port.DataReceived += PortOnDataReceived;
            try
            {
                Port.DtrEnable = true;
                Port.Parity=Parity.None;
                Port.NewLine = ";";
          
                Port.Open();
                Connected = true;
            }
            catch (Exception exception)
            {
                Port.Dispose();
                Connected = false;
            }
            return Connected;
        }

        public void CloseSerial()
        {
            Connected = false;
            Port?.Close();
            Port?.Dispose();
            Port = null;
        }

        #endregion Innitializer

        #region Singleton

        internal static IoSerial SingletonContext { get; set; }

        private static readonly object IOSerialSingletonLocker = new object();
        private bool _connected=false;

        public static IoSerial Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (IOSerialSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new IoSerial();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton
    }
}