using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveDevicesServiceAndroid))]
namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
    public class SaveDevicesServiceAndroid : ISaveDevicesService
    {
        private static readonly JsonSerializerSettings
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        public async Task<IEnumerable<ScannedDeviceInfo>> LoadDevices()
        {
            var devicesInfo = new List<ScannedDeviceInfo>();

            await Task.Run(() =>
            {
                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), "SavedDevices.json");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    return;
                }

                var file = new StreamReader(backingFile, true);

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
                                        string address = readDevice.DeviceAddress;
                                        if (address == null) break;
                                        devicesInfo.Add(new ScannedDeviceInfo(
                                            readDevice.DeviceName, address, BluetoothType.Classic));
                                        break;
                                    case BluetoothType.Le:
                                        string addressLe = readDevice.DeviceAddress;
                                        if (addressLe == null) break;
                                        if (Guid.TryParse(addressLe, out Guid addressGuid))
                                        {
                                            devicesInfo.Add(new ScannedDeviceInfo(
                                                readDevice.DeviceName, addressGuid, BluetoothType.Le));
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

        public async Task SaveDevices(IEnumerable<ScannedDeviceInfo> devices)
        {
            await Task.Run(() =>
            {
                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal),
                    "SavedDevices.json");

                using (var file = File.CreateText(backingFile))
                {
                    foreach (var device in devices)
                    {
                        switch (device.BluetoothType)
                        {
                            case BluetoothType.Classic:
                                if (device.BluetoothArgs is string address)
                                {
                                    var savedDevice = new SavedDevice
                                    {
                                        DeviceAddress = address,
                                        DeviceName = device.Name,
                                        BluetoothType = BluetoothType.Classic
                                    };
                                    var jsonString = JsonConvert.SerializeObject(
                                        savedDevice, _settings);

                                    file.WriteLine(jsonString);
                                }
                                break;
                            case BluetoothType.Le:
                                if (device.BluetoothArgs is Guid addressGuid)
                                {
                                    var savedDevice = new SavedDevice
                                    {
                                        DeviceAddress = addressGuid.ToString(),
                                        DeviceName = device.Name,
                                        BluetoothType = BluetoothType.Le
                                    };
                                    var jsonString = JsonConvert.SerializeObject(
                                        savedDevice, _settings);
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
    }
}