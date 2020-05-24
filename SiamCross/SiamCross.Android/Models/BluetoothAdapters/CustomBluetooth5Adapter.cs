using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models;
using SiamCross.Models.USB;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    public class CustomBluetooth5Adapter : IBluetoothAdapter, IUsbObserver
    {
        public string Address { get; private set; }

        public CustomBluetooth5Adapter(string address)
        {
            Address = address;
        }

        public Task Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task SendData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Update(byte[] data)
        {
            DataReceived?.Invoke(data);
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;
    }
}