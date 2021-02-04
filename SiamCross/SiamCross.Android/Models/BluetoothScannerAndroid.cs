using Android.Bluetooth;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SiamCross.Droid.Models;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;

[assembly: Dependency(typeof(BluetoothScannerAndroid))]
namespace SiamCross.Droid.Models
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class BluetoothScannerAndroid : IBluetoothScanner
    {
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetoothBLE;
        public event Action<ScannedDeviceInfo> Received;
        public event Action ScanTimoutElapsed;

        public BluetoothScannerAndroid()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.ScanTimeout = 10000;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.ScanTimeoutElapsed += (s, e) => { ScanTimoutElapsed?.Invoke(); };

            _adapter.DeviceDiscovered += (obj, a) =>
            {
                if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                    return;
                ScannedDeviceInfo sd = new ScannedDeviceInfo
                {
                    Name = a.Device.Name,
                    Mac = a.Device.Id.ToString(),
                    Id = a.Device.Id,
                    BluetoothType = BluetoothType.Le
                };
                Received?.Invoke(sd);
                System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
            };

        }

        private void StartBounded()
        {
            ICollection<BluetoothDevice> devices = BluetoothAdapter.DefaultAdapter.BondedDevices;

            foreach (BluetoothDevice device in devices)
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
                bool parsed = Guid.TryParse(mac_no_delim, out Guid id);
                if (null == id)
                    id = new Guid();

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
                ScannedDeviceInfo sd = new ScannedDeviceInfo
                {
                    Name = device.Name,
                    Mac = device.Address,
                    Id = id,
                    BluetoothType = bluetoothType
                };
                Received?.Invoke(sd);
            }

        }
        public void Start()
        {
            StartBounded();
            StartLE();
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

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}