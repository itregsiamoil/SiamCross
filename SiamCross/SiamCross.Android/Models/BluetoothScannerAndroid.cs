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
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetoothBLE;
        private readonly BluetoothAdapter _socketAdapter;
        private readonly USBService _usbService;
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

                Received?.Invoke(new ScannedDeviceInfo(a.Device.Name, a.Device.Id, BluetoothType.Le, a.Device.Id.ToString()));
                System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
            };

            _usbService = USBService.Instance;
            _usbService.DeviceFounded += UsbService_DeviceFounded;
        }

        private void UsbService_DeviceFounded(ScannedDeviceInfo scannedDeviceInfo)
        {
            Received?.Invoke(scannedDeviceInfo);
        }

        public void Start()
        {
            ICollection<BluetoothDevice> devices = _socketAdapter.BondedDevices;

            foreach (var device in devices)
            {
                string mac_no_delim = device.Address;
                int exist = mac_no_delim.IndexOf(':');
                //"00000000-0000-0000-0000-0016a4720012"
                while (0 < exist)
                {
                    mac_no_delim = mac_no_delim.Remove(exist, 1);
                    exist = mac_no_delim.IndexOf(':');
                }
                mac_no_delim = "00000000-0000-0000-0000-" + mac_no_delim;
                Guid id = new Guid();
                bool parsed = Guid.TryParse(mac_no_delim, out id);

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
                Received?.Invoke(new ScannedDeviceInfo(device.Name, id, bluetoothType, device.Address));
            }

            StartLE();
            //StartScanUsb();
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

        private async void StartScanUsb()
        {
            await _usbService.StartScanQuery();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}