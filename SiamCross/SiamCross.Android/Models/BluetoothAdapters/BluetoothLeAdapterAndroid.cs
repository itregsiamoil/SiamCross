﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

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
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services.Logging;
using Plugin.BLE.Abstractions;
using Android.Bluetooth;

[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    public class BluetoothLeAdapterAndroid : IBluetoothLeAdapter
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

        private List<string> _connectQueue;

        public BluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo)
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _deviceInfo = deviceInfo;
            _deviceGuid = (Guid)deviceInfo.BluetoothArgs;
            

            if (_connectQueue == null)
            {
                _connectQueue = new List<string>();
            }
        }

        public async Task Connect()
        {
            if(!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                return;
            }
            if (_connectQueue.Contains(_deviceInfo.Name))
            {
                return;
            }
            else
            {
                _connectQueue.Add(_deviceInfo.Name);
            }

            if (_adapter == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения" +
                    " _adapter == null. Будет произведена переинициализация адаптера");
                await Disconnect();
                _adapter = CrossBluetoothLE.Current.Adapter;
                _connectQueue.Remove(_deviceInfo.Name);
                return;
            }

            try
            {
                await _adapter.ConnectToKnownDeviceAsync(_deviceGuid);
            }
            catch (AggregateException e)
            {
                System.Diagnostics.Debug.WriteLine("Истек таймаут подключения "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
                return;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
                return;
            }

            if (_adapter != null)
            {
                _device = _adapter.ConnectedDevices.Where(x => x.Id == _deviceGuid)
                    .LastOrDefault();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения" +
                    " _adapter == null. Будет произведена переинициализация адаптера");
                _adapter = CrossBluetoothLE.Current.Adapter;
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
                return;
            }

            if (_device == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                    + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                ConnectFailed();
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
                return;
            }
            await Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                _targetService = await _device.GetServiceAsync(Guid.Parse(_serviceGuid));

                var serv = await _targetService.GetCharacteristicsAsync();
                foreach (var ch in serv)
                {
                    System.Diagnostics.Debug.WriteLine(ch.Id);
                }

                _writeCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_writeCharacteristicGuid));
                _readCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_readCharacteristicGuid));
                _readCharacteristic.ValueUpdated += (o, args) =>
                {
                    DataReceived?.Invoke(args.Characteristic.Value);
                    System.Diagnostics.Debug.WriteLine("Recieved: " + BitConverter.ToString(args.Characteristic.Value) + "\n");
                };

                await _readCharacteristic.StartUpdatesAsync();
                ConnectSucceed();
                _connectQueue.Remove(_deviceInfo.Name);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect "
                    + _deviceInfo.Name + " ошибка инициализации: " + e.Message);
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
            }
        }

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public async Task SendData(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine("Send: " + BitConverter.ToString(data) + "\n");
            try
            {
                await _writeCharacteristic.WriteAsync(data);
            }
            catch (Exception sendingEx)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка отправки сообщения BLE: "
                    + BitConverter.ToString(data)
                    + " " + sendingEx.Message + " " 
                    + sendingEx.GetType() + " " 
                    + sendingEx.StackTrace + "\n");
                for (int i = 1; i < 11; i++)
                {
                    try
                    {
                        await Task.Delay(500);
                        await _writeCharacteristic.WriteAsync(data);
                        System.Diagnostics.Debug.WriteLine(
                            $"Повторная попытка отправки номер {i}/10 прошла успешно!" + "\n");
                        return;
                    }
                    catch (Exception resendingEx)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Ошибка повторной попытки отправки номер {i}/10 сообщения BLE: " + "\n");
                    }
                }
                // Возможно нужно сделать дисконект
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

                _device?.Dispose();

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