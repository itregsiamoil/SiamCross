using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public class ScannedDevicesService : IScannedDevicesService
    {
        private List<ScannedDeviceInfo> _devices;

        private readonly IBluetoothScanner _scanner;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Test()
        {
            _scanner.Test();
        }

        public ScannedDevicesService(IBluetoothScanner scanner)
        {
            _scanner = scanner;
            _scanner.Received += ScannerReceived;
            _devices = new List<ScannedDeviceInfo>();
        }

        private void ScannerReceived(ScannedDeviceInfo info)
        {
            if (info.Name == null || info.BluetoothArgs == null || info.Name == "")
            {
                return;
            }


            if (!_devices.Contains(info))
            {
                _devices.Add(info);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScannedDevices)));
            }
        }

        public IEnumerable<ScannedDeviceInfo> ScannedDevices
        {
            get => _devices;
        }

        public void StartScan()
        {
            _scanner.Start();
        }

        public void StopScan()
        {
            _scanner.Stop();            
        }
    }
}
