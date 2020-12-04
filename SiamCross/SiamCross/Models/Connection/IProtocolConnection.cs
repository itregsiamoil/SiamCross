using SiamCross.Models.Adapters;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public enum ConnectionState
    {
        Disconnected = 0,
        PendingConnect = 1,
        Connected = 2,
        PendingDisconnect = 3
    }
    public interface IProtocolConnection
    {
        void UpdateRssi();
        int Rssi { get; }
        ConnectionState State { get; }
        IPhyInterface PhyInterface { get; }

        Task<bool> Connect();
        Task Disconnect();
        Task SendData(byte[] data);
        event Action<byte[]> DataReceived;
        event Action ConnectSucceed;
        event Action ConnectFailed;

        Task<byte[]> Exchange(byte[] req);

        void DoActionDataReceived(byte[] data);
        void DoActionConnectSucceed();
        void DoActionConnectFailed();
    }


}