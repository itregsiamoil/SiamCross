using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;
using Plugin.BLE.Abstractions;
using SiamCross.Models;
using Xamarin.Forms;
using SiamCross.Droid.Models;
using SiamCross.Models.Scanners;
using System.Threading.Tasks;
using System.Threading;
using SiamCross.Models.USB;

[assembly: Dependency(typeof(BluetoothScannerAndroid))]
namespace SiamCross.Droid.Models
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class BluetoothScannerAndroid : IBluetoothScanner
    {
        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        private BluetoothAdapter _socketAdapter;
        private USBService _usbService;
        public event Action<ScannedDeviceInfo> Received;
        public event Action ScanTimoutElapsed;

        public BluetoothScannerAndroid()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _socketAdapter = BluetoothAdapter.DefaultAdapter;
            _adapter.ScanTimeout = 10000;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.ScanTimeoutElapsed += (s, e) => { ScanTimoutElapsed?.Invoke(); };

            _adapter.DeviceDiscovered += (obj, a) =>
            {
                if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                {
                    return;
                }

                Received?.Invoke(new ScannedDeviceInfo(a.Device.Name, a.Device.Id, BluetoothType.Le));
                System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
            };

            _usbService = USBService.Instance;
            _usbService.DeviceFounded += _usbService_DeviceFounded;
        }

        private void _usbService_DeviceFounded(ScannedDeviceInfo scannedDeviceInfo)
        {
            Received?.Invoke(scannedDeviceInfo);
        }

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

            StartLE();
            StartScanUsb();
        }

        private async void StartLE()
        {
            if (_bluetoothBLE.State != BluetoothState.Off)
            {
                if (!_bluetoothBLE.Adapter.IsScanning)
                {
                    await _adapter.StartScanningForDevicesAsync();
                }
            }
        }

        private void StartScanUsb()
        {
            _usbService.StartScanQuery();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}