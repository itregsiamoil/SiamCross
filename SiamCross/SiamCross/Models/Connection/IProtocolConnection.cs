using SiamCross.Models.Adapters;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class IOEx_Timeout : Exception
    {
        public IOEx_Timeout() { }
        public IOEx_Timeout(string message) : base(message) { }
        public IOEx_Timeout(string message, Exception inner) : base(message, inner) { }
    }
    public class IOEx_WriteTimeout : IOEx_Timeout
    {
        public IOEx_WriteTimeout() { }
        public IOEx_WriteTimeout(string message) : base(message) {}
        public IOEx_WriteTimeout(string message, Exception inner): base(message, inner){}
    }
    public class IOEx_ReadTimeout : IOEx_Timeout
    {
        public IOEx_ReadTimeout() { }
        public IOEx_ReadTimeout(string message) : base(message) { }
        public IOEx_ReadTimeout(string message, Exception inner) : base(message, inner) { }
    }
    public class IOEx_ErrorResponse : Exception
    {
        public IOEx_ErrorResponse() { }
        public IOEx_ErrorResponse(string message) : base(message) { }
        public IOEx_ErrorResponse(string message, Exception inner) : base(message, inner) { }
    }

    public enum ConnectionState
    {
        Disconnected = 0,
        PendingConnect = 1,
        Connected = 2,
        PendingDisconnect = 3
    }
    static public class ConnectionStateAdapter
    {
        static public string ToString(ConnectionState conn)
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

        event PropertyChangedEventHandler PropertyChanged;
    }


}