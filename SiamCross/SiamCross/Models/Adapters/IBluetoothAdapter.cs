﻿using System;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IBluetoothAdapter
    {
        Task Connect();

        Task SendData(byte[] data);

        event Action<byte[]> DataReceived;

        Task Disconnect();
    }
}