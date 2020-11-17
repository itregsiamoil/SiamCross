﻿using SiamCross.Models.Adapters;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IProtocolConnection
    {
        int State { get; }
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