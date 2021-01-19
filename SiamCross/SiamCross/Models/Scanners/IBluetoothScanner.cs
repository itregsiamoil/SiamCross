using System;

namespace SiamCross.Models.Scanners
{
    public interface IBluetoothScanner
    {
        void Start();

        void Stop();

        event Action<ScannedDeviceInfo> Received;

        event Action ScanTimoutElapsed;
    }
}
