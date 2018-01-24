using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagioDevice
{
    public class DeviceInfo
    {

        #region Singleton

        internal static DeviceInfo SingletonContext { get; set; }

        private static readonly object DeviceInfoSingletonLocker = new object();

        public static DeviceInfo Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (DeviceInfoSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new DeviceInfo();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        
    }
}
