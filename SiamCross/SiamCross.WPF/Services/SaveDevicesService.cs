using InTheHand.Net;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.WPF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveDevicesServicePC))]
namespace SiamCross.WPF.Services
{
    public class SaveDevicesServicePC : ISaveDevicesService
    {
        private static readonly JsonSerializerSettings
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        public async Task SaveDevices(IEnumerable<ScannedDeviceInfo> devices)
        {
            await Task.Run(()=>
            {
                using (var file = new StreamWriter(
                    Path.Combine(Directory.GetCurrentDirectory(), "SavedDevices.json")))
                {
                    foreach (var device in devices)
                    {
                        switch (device.BluetoothType)
                        {
                            case BluetoothType.Classic:
                                if (device.BluetoothArgs is BluetoothDeviceInfo argsClassic)
                                {
                                    var savedDevice = new SavedDevice
                                    {
                                        BluetoothType = BluetoothType.Classic,
                                        DeviceName = argsClassic.DeviceName,
                                        DeviceAddress = argsClassic.DeviceAddress.ToString()
                                    };
                                    var jsonString = JsonConvert.SerializeObject(savedDevice,
                                        _settings);
                                    file.WriteLine(jsonString);
                                }
                                break;
                            case BluetoothType.Le:
                                if (device.BluetoothArgs is ulong LEAddress)
                                {
                                    var savedDevice = new SavedDevice
                                    {
                                        BluetoothType = BluetoothType.Le,
                                        DeviceName = device.Name,
                                        DeviceAddress = LEAddress.ToString()
                                    };
                                    var jsonString = JsonConvert.SerializeObject(savedDevice,
                                        _settings);
                                    file.WriteLine(jsonString);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            });           
        }

        public async Task<IEnumerable<ScannedDeviceInfo>> LoadDevices()
        {
            //throw new NotImplementedException();
            var devicesInfo = new List<ScannedDeviceInfo>();

            await Task.Run(() =>
            {
                var file = new StreamReader(
                    Path.Combine(Directory.GetCurrentDirectory(), "SavedDevices.json"));

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();

                        object item = JsonConvert.DeserializeObject(
                            line, _settings);

                        switch (item)
                        {
                            case SavedDevice readDevice:
                                switch (readDevice.BluetoothType)
                                {
                                    case BluetoothType.Classic:
                                        BluetoothAddress addr = BluetoothAddress.Parse(readDevice.DeviceAddress);
                                        if (addr == null) break;
                                        BluetoothDeviceInfo deviceArgs = new BluetoothDeviceInfo(addr);
                                        if (deviceArgs == null) break;
                                        devicesInfo.Add(new ScannedDeviceInfo(
                                            readDevice.DeviceName, deviceArgs, BluetoothType.Classic));
                                        break;
                                    case BluetoothType.Le:
                                        if (UInt64.TryParse(
                                            readDevice.DeviceAddress, out ulong address))
                                        {
                                            devicesInfo.Add(new ScannedDeviceInfo(
                                                readDevice.DeviceName, address, BluetoothType.Le));
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                file.Close();
            });

            return devicesInfo;
        }
    }
}
