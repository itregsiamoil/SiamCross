using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Mono.Data.Sqlite;

namespace SiamCross.Models.USB
{
    public interface IUsbDataObserver
    {
        String Address { get; }

        void Update(byte[] data);
    }
}
