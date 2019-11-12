using System;
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
using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothLeAdapterMobile))]
namespace SiamCross.Droid.Models
{
    public class BluetoothLeAdapterMobile : IBluetoothAdapter
    {
        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        private IDevice _device;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;
        private string _addres;

        private const string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167";
        private const string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167";
        private const string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";

        public BluetoothLeAdapterMobile()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
        }

        public async Task Connect(object connectArgs)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (connectArgs is string)
                    {
                        await ConnectG(connectArgs as string);
                    }
                    else if (connectArgs is IDevice)
                    {
                        await ConnectD(connectArgs as IDevice);
                    }
                    else
                    {
                        throw new Exception("Invalid argument!");
                    }
                    await Task.Delay(2000);
                    break;
                }
                catch
                {
                    await Task.Delay(2000);
                }
            }
        }

        private async Task ConnectD(IDevice device)
        {
            try
            {
                if (device != null)
                {
                    await _adapter.ConnectToDeviceAsync(device);
                    await Initialize();
                }
                else
                {
                    //DisplayAlert("Notice", "No Device selected !", "OK");
                }
            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                // DisplayAlert("Notice", ex.Message.ToString(), "OK");
            }
        }

        private async Task ConnectG(string guidString)
        {
            var guid = new Guid(guidString);
            try
            {
                await _adapter.ConnectToKnownDeviceAsync(guid);
                await Initialize();

            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                //DisplayAlert("Notice", ex.Message.ToString(), "OK");
            }
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
                
            }
            
        }

        public event Action<byte[]> DataReceived;
    }
}