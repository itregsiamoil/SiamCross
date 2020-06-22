using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Mono.Data.Sqlite;

namespace SiamCross.Models.USB
{
    public interface IUsbDataObserver
    {
        string Address { get; }

        void OnDataRecieved(byte[] data);

        void OnConnectSucceed();

        void OnDisconnected();
    }
}
