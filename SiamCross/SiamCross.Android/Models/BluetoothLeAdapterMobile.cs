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
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private const string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167";
        private const string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167";
        private const string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private ScannedDeviceInfo _deviceInfo;

        public BluetoothLeAdapterMobile(ScannedDeviceInfo deviceInfo)
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _deviceInfo = deviceInfo;
        }

        public async Task Connect()
        {


            //try
            //{
            //    System.Diagnostics.Debug.WriteLine("Запуск процедуры подключения LE адаптера");
            //    _device = (IDevice)_deviceInfo.BluetoothArgs;
            //    System.Diagnostics.Debug.WriteLine("Device привелся к своему типу");
            //    if (_device == null)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Device был null");
            //        return;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("Device был НЕ null");
            //    }
                
            //    await _adapter.ConnectToDeviceAsync(_device);
            //    await Initialize();
            //    ConnectSucceed?.Invoke();
            //}
            //catch (DeviceConnectionException ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Ошибка соединения LE адаптера DeviceConnectionExcezion");
            //}
            //catch(Exception e)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Исключение{e.Message}");
            //    throw new Exception();
            //}
        }

        public async Task SendData(byte[] data)
        { 
            await _writeCharacteristic.WriteAsync(data);
        }

        private async Task Initialize()
        {
            _targetService = await _device.GetServiceAsync(new Guid(_serviceGuid));

            _writeCharacteristic = await _targetService.GetCharacteristicAsync(new Guid(_writeCharacteristicGuid));
            _readCharacteristic = await _targetService.GetCharacteristicAsync(new Guid(_readCharacteristicGuid));
            _readCharacteristic.ValueUpdated += (o, args) =>
            {
                DataReceived?.Invoke(args.Characteristic.Value);
            };

            await _readCharacteristic.StartUpdatesAsync();
        }

        public async Task Disconnect()
        {
            if (_device != null)
            {
                _writeCharacteristic = null;
                _readCharacteristic = null;
                _adapter = null;

                _device.Dispose();
                _targetService.Dispose();

                _device = null;
                _targetService = null;
            }
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;///////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!
    }
}