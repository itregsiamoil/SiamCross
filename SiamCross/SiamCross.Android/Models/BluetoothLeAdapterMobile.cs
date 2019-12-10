﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using SiamCross.Droid.Models;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Adapters;
using Xamarin.Forms;
using Plugin.BLE.Abstractions.EventArgs;

[assembly: Dependency(typeof(BluetoothLeAdapterMobile))]
namespace SiamCross.Droid.Models
{
    public class BluetoothLeAdapterMobile : IBluetoothLeAdapter
    {
        private IAdapter _adapter;
        private IDevice _device;
        private Guid _deviceGuid;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private const string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167c";
        private const string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167c";
        private const string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private ScannedDeviceInfo _deviceInfo;

        public BluetoothLeAdapterMobile(ScannedDeviceInfo deviceInfo)
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _deviceInfo = deviceInfo;
            _deviceGuid = (Guid)deviceInfo.BluetoothArgs;
        }

        public async Task Connect()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _adapter.StopScanningForDevicesAsync();
                try
                {
                    await _adapter.ConnectToKnownDeviceAsync(_deviceGuid);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                        + _deviceInfo.Name + ": " + e.Message);
                }

                _device = _adapter.ConnectedDevices.Where(x => x.Id == _deviceGuid)
                    .LastOrDefault();
                if (_deviceGuid == null)
                {
                    System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                        + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                    ConnectFailed();
                    return;
                }
                await Initialize();
            });
        }

        private async Task Initialize()
        {
            try
            {
                _targetService = await _device.GetServiceAsync(Guid.Parse(_serviceGuid));

                var serv = await _targetService.GetCharacteristicsAsync();
                foreach(var ch in serv)
                {
                    System.Diagnostics.Debug.WriteLine(ch.Id);
                }

                _writeCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_writeCharacteristicGuid));
                _readCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_readCharacteristicGuid));
                _readCharacteristic.ValueUpdated += (o, args) =>
                {
                    DataReceived?.Invoke(args.Characteristic.Value);
                };

                await _readCharacteristic.StartUpdatesAsync();
                ConnectSucceed();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect " 
                    + _deviceInfo.Name + " ошибка инициализации: " + e.Message);
                await Disconnect();
            }
        }

        public async Task SendData(byte[] data)
        {
            try
            {
                await _writeCharacteristic.WriteAsync(data);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка отправки сообщения BLE: " + e.Message);
                ConnectFailed();
            }
        }

        public async Task Disconnect()
        {
            if (_deviceGuid != null)
            {
                _writeCharacteristic = null;
                _readCharacteristic = null;
                _adapter = null;

                _device.Dispose();
                _targetService?.Dispose();

                _device = null;
                _targetService = null;
            }
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;///////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!
    }
}