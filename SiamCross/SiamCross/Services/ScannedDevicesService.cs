using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.Plugin.Messenger;
using SiamCross.Models.Scanners;

namespace SiamCross.Services
{
    public class ScannedDevicesService : IScannedDevicesService
    {
        private List<ScannedDeviceInfo> _devices;

        private readonly IBluetoothScanner _scanner;

        private readonly IMvxMessenger _messenger;

        public ScannedDevicesService(IBluetoothScanner scanner,
                                     IMvxMessenger messenger)
        {
            _scanner = scanner;
            _messenger = messenger;
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
                _messenger.Publish(new ScannedDevicesListChangedMessage(this));
            }
        }

        public IEnumerable<ScannedDeviceInfo> GetScannedDevices()
        {
            return _devices;
        }
    }
}
