using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Scanners
{
    public interface IBluetoothScanner
    {
        void Start();

        void Stop();

        event Action<ScannedDeviceInfo> Received;
    }
}
