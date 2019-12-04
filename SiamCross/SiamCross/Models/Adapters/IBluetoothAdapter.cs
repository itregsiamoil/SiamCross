using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IBluetoothAdapter
    {
        Task Connect();
        Task Disconnect();
        Task SendData(byte[] data);
        event Action<byte[]> DataReceived;
        event Action ConnectSucceed;
        event Action ConnectFailed;
    }
}