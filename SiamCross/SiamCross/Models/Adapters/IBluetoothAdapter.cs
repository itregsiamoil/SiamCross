using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models
{
    [Preserve(AllMembers = true)]
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