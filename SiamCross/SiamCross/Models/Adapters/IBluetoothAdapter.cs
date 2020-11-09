using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IBluetoothAdapter
    {
        Task<bool> Connect();
        Task Disconnect();
        Task SendData(byte[] data);
        event Action<byte[]> DataReceived;
        event Action ConnectSucceed;
        event Action ConnectFailed;

        Task<byte[]> Exchange(byte[] req);
    }
}