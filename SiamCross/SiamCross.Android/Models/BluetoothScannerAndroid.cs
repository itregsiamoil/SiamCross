using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;
using SiamCross.Models;
using Xamarin.Forms;
using SiamCross.Droid.Models;
using SiamCross.Models.Scanners;
using System.Threading.Tasks;
using System.Threading;
using Plugin.BLE.Abstractions;

[assembly: Dependency(typeof(BluetoothScannerAndroid))]
namespace SiamCross.Droid.Models
{
    public class BluetoothScannerAndroid : IBluetoothScanner
    {
        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        private BluetoothAdapter _socketAdapter;
        private List<IDevice> _deviceList = new List<IDevice>();

        public BluetoothScannerAndroid()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _socketAdapter = BluetoothAdapter.DefaultAdapter;
        }

        public event Action<ScannedDeviceInfo> Received;

        public void Start()
        {
            ICollection<BluetoothDevice> devices = _socketAdapter.BondedDevices;

            foreach (var device in devices)
            {
                BluetoothType bluetoothType = BluetoothType.Le;
                switch (device.Type)
                {
                    case BluetoothDeviceType.Classic:
                        bluetoothType = BluetoothType.Classic;
                        break;
                    case BluetoothDeviceType.Le:
                        bluetoothType = BluetoothType.Le;
                        break;
                }


                Received?.Invoke(new ScannedDeviceInfo(device.Name, device.Address, bluetoothType));
            }

            StartScann();
        }

        private async void StartScann()
        {
            if (_bluetoothBLE.State == BluetoothState.Off)
            {
            }
            else
            {
                //_adapter.ScanTimeout = 10000;
                //_adapter.ScanMode = ScanMode.Balanced;

                _adapter.DeviceDiscovered += (obj, a) =>
                {
                    if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                    {
                        return;
                    }

                    Received?.Invoke(new ScannedDeviceInfo(a.Device.Name, a.Device.Id, BluetoothType.Le));
                    System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
                };

                if (!_bluetoothBLE.Adapter.IsScanning)
                {
                    await _adapter.StartScanningForDevicesAsync();
                }
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}