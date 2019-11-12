using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IBluetoothAdapter
    {
        Task Connect(object connectArgs);

        Task SendData(byte[] data);

        event Action<byte[]> DataReceived;

        Task Disconnect();
    }
}