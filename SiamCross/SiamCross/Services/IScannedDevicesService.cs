using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public interface IScannedDevicesService : INotifyPropertyChanged
    {
        void StartScan();

        void StopScan();

        IEnumerable<ScannedDeviceInfo> ScannedDevices { get; }
    }
}
