using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.WPF.Models
{
    public class BluetoothAdapterClassicPC : IBluetoothAdapter
    {


        public event Action<byte[]> DataReceived;

        public Task Connect(object connectArgs)
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
    }
}
