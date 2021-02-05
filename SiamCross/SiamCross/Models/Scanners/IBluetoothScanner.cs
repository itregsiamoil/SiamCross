using System;

namespace SiamCross.Models.Scanners
{
    public interface IBluetoothScanner
    {
        void Start();

        void Stop();

        void StartBounded();
        bool ActiveScan { get; }
        string ScanString { get; }

        event Action<ScannedDeviceInfo> Received;
        event Action ScanStarted;
        event Action ScanStoped;
    }
}
