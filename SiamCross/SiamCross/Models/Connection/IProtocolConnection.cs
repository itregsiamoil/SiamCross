using System;
using System.ComponentModel;
using System.Threading;
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
    public static class ConnectionStateAdapter
    {
        public static string ToString(ConnectionState conn)
        {
            switch (conn)
            {
                default:
                case ConnectionState.Disconnected: return Resource.StatConn_Disconnected;
                case ConnectionState.PendingConnect: return Resource.StatConn_PendingConnect;
                case ConnectionState.Connected: return Resource.StatConn_Connected;
                case ConnectionState.PendingDisconnect: return Resource.StatConn_PendingDisconnect;
            }
        }
    }

    public interface IProtocolConnection
    {
        void UpdateRssi();
        int Rssi { get; }
        ConnectionState State { get; }
        IPhyConnection PhyConnection { get; }

        Task<bool> Connect();
        Task<bool> Disconnect();

        Task<byte[]> Exchange(byte[] req);

        event PropertyChangedEventHandler PropertyChanged;

        /*
        Task SendData(byte[] data);
        event Action<byte[]> DataReceived;
        event Action ConnectSucceed;
        event Action ConnectFailed;

        void DoActionDataReceived(byte[] data);
        void DoActionConnectSucceed();
        void DoActionConnectFailed();
        */

    }


}