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

[assembly: Dependency(typeof(BluetoothScannerMobile))]
namespace SiamCross.Droid.Models
{
    public class BluetoothScannerMobile : IBluetoothScanner
    {
        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        private BluetoothAdapter _socketAdapter;

        public BluetoothScannerMobile()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _socketAdapter = BluetoothAdapter.DefaultAdapter;
        }

        public event Action<string, object> Received;

        public void Start()
        {
            ICollection<BluetoothDevice> devices = _socketAdapter.BondedDevices;

            foreach (var device in devices)
            {
                Received?.Invoke(device.Name, devices);
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
                _adapter.ScanTimeout = 10000;
                _adapter.ScanMode = ScanMode.Balanced;

                _adapter.DeviceDiscovered += (obj, a) =>
                {
                    Received?.Invoke(a.Device.Name, a.Device);
                };

                await _adapter.StartScanningForDevicesAsync();
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}