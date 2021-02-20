using SiamCross.Models.Adapters;
using System;
using System.ComponentModel;

namespace SiamCross.Models.Scanners
{
    public interface IBluetoothScanner : INotifyPropertyChanged
    {
        void Start();
        void Stop();

        bool ActiveScan { get; }
        string ScanString { get; }

        event Action<ScannedDeviceInfo> Received;
        event Action ScanStarted;
        event Action ScanStoped;

        IPhyInterface Phy { get; }
    }

    public interface IScannerBt2 : IBluetoothScanner { }
    public interface IScannerLe : IBluetoothScanner { }
}
