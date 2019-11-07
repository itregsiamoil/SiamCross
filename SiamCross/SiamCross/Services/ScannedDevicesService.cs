using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.Models.Scanners;

namespace SiamCross.Services
{
    public class ScannedDevicesService : IScannedDevicesService
    {
        private List<ScannedDeviceInfo> _devices;

        private readonly IBluetoothScanner _scanner;

        public ScannedDevicesService(IBluetoothScanner scanner)
        {
            _scanner = scanner;
            _scanner.Received += ScannerReceived;
            _devices = new List<ScannedDeviceInfo>();
        }

        private void ScannerReceived(string name, object bluetoothArgs)
        {
            if (name == null || bluetoothArgs == null)
            {
                return;
            }

            var deviceInfo = new ScannedDeviceInfo(name, bluetoothArgs);

            if (!_devices.Contains(deviceInfo))
            {
                _devices.Add(deviceInfo);
            }
        }

        public IEnumerable<ScannedDeviceInfo> GetScannedDevices()
        {
            return _devices;
        }
    }
}
