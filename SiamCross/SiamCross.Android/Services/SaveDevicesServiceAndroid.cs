using Android.Runtime;
using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveDevicesServiceAndroid))]
namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
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
            List<ScannedDeviceInfo> devicesInfo = new List<ScannedDeviceInfo>();

            string backingFile = Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal), "SavedDevices.json");

            if (backingFile == null || !File.Exists(backingFile))
            {
                return null;
            }

            lock (_locker)
            {
                StreamReader file = new StreamReader(backingFile, true);

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        string line = file.ReadLine();

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
            string backingFile = Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal),
                "SavedDevices.json");

            lock (_locker)
            {
                using (StreamWriter file = File.CreateText(backingFile))
                {
                    foreach (ScannedDeviceInfo device in devices)
                    {
                        SavedDevice savedDevice = new SavedDevice
                        {
                            Id = device.Id.ToString(),
                            Mac = device.Mac,
                            DeviceName = device.Name,
                            BluetoothType = device.BluetoothType
                        };
                        string jsonString = JsonConvert.SerializeObject(
                            savedDevice, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }
    }
}