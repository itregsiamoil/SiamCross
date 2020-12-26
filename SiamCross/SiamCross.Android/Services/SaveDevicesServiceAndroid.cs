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
    public class SaveDevicesServiceAndroid : ISaveDevicesService
    {
        private readonly object _locker = new object();

        private static readonly JsonSerializerSettings
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        /// <summary>
        /// Can return null
        /// </summary>
        /// <returns></returns>
        public List<ScannedDeviceInfo> LoadDevices()
        {
            var devicesInfo = new List<ScannedDeviceInfo>();

            var backingFile = Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal), "SavedDevices.json");

            if (backingFile == null || !File.Exists(backingFile))
            {
                return null;
            }

            lock (_locker)
            {
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
                                Guid id = new Guid();
                                Guid.TryParse(readDevice.Id, out id);

                                devicesInfo.Add(new ScannedDeviceInfo(
                                            readDevice.DeviceName, id
                                            , readDevice.BluetoothType, readDevice.Mac));
                                break;
                            default:
                                break;
                        }
                    }
                }
                file.Close();
            }

            return devicesInfo;
        }

        public void SaveDevices(IEnumerable<ScannedDeviceInfo> devices)
        {
            var backingFile = Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal),
                "SavedDevices.json");

            lock (_locker)
            {
                using (var file = File.CreateText(backingFile))
                {
                    foreach (var device in devices)
                    {
                        var savedDevice = new SavedDevice
                        {
                            Id = device.Id.ToString(),
                            Mac = device.Mac,
                            DeviceName = device.Name,
                            BluetoothType = device.BluetoothType
                        };
                        var jsonString = JsonConvert.SerializeObject(
                            savedDevice, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }
    }
}