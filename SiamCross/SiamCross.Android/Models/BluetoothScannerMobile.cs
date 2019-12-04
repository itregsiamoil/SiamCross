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

[assembly: Dependency(typeof(BluetoothScannerMobile))]
namespace SiamCross.Droid.Models
{
    public class BluetoothScannerMobile : IBluetoothScanner
    {
        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        private BluetoothAdapter _socketAdapter;
        private List<IDevice> _deviceList = new List<IDevice>();

        public BluetoothScannerMobile()
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
                _adapter.ScanTimeout = 10000;
                _adapter.ScanMode = ScanMode.Balanced;

                _adapter.DeviceDiscovered += (obj, a) =>
                {
                    if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                    {
                        return;
                    }

                    Received?.Invoke(new ScannedDeviceInfo(a.Device.Name, a.Device, BluetoothType.Le));
                    System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);

                    if (a.Device.Name.Contains("170"))
                    {
                        _device = a.Device;
                        _deviceList.Add(_device);
                        // Initialize();
                    }
                };

                await _adapter.StartScanningForDevicesAsync();
            }
        }

        private IDevice _device;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private const string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167";
        private const string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167";
        private const string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private ScannedDeviceInfo _deviceInfo;

        public async Task Test()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _adapter.StopScanningForDevicesAsync();
                    var connectParams = new ConnectParameters(true, true);
                    _adapter.ConnectToKnownDeviceAsync(_deviceList[0].Id, connectParams);
                });

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            await Task.Delay(5000);
            // await Task.Delay(3000);
            IReadOnlyList<IService> qwe;
            try
            {
                qwe = _deviceList[0].GetServicesAsync().Result;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            await Task.Delay(5000);
            var asdasd = _adapter.ConnectedDevices.Count;
        }

        private void Initialize()
        {
            //  try
            // {
            //_targetService = await _device.GetServiceAsync(Guid.Parse(_serviceGuid));
            //IService asd = await _device.GetServiceAsync(Guid.Parse(_serviceGuid));
            try
            {
                var connectParams = new ConnectParameters(true, true);
                _adapter.StopScanningForDevicesAsync();
                _adapter.ConnectToKnownDeviceAsync(_deviceList[0].Id, connectParams);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            // await Task.Delay(3000);
            IReadOnlyList<IService> qwe = _device.GetServicesAsync().Result;
            // }
            // catch (Exception e)
            // {
            //   System.Diagnostics.Debug.WriteLine(e.Message);
            // }

            //_writeCharacteristic = await _targetService.GetCharacteristicAsync(new Guid(_writeCharacteristicGuid));
            //_readCharacteristic = await _targetService.GetCharacteristicAsync(new Guid(_readCharacteristicGuid));
            //_readCharacteristic.ValueUpdated += (o, args) =>
            //{
            //    //DataReceived?.Invoke(args.Characteristic.Value);
            //};

            //await _readCharacteristic.StartUpdatesAsync();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}