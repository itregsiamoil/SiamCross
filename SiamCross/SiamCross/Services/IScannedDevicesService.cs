using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiamCross.Services
{
    public interface IScannedDevicesService : INotifyPropertyChanged
    {
        void StartScan();

        void StopScan();

        IEnumerable<ScannedDeviceInfo> ScannedDevices { get; }
    }
}
