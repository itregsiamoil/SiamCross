using Android.Runtime;
using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
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
                            case SavedDevice read:
                                Guid id = new Guid();
                                Guid.TryParse(read.Id, out id);

                                ScannedDeviceInfo sd = new ScannedDeviceInfo();
                                sd.Device.Kind = read.Kind;
                                sd.Device.Name = read.DeviceName;
                                
                                sd.Device.PhyId = (uint)read.BluetoothType;
                                sd.Device.PhyData["Mac"]= read.Mac;
                                sd.Device.PhyData["Guid"]= read.Id;

                                sd.Device.ProtocolId = read.ProtocolId;
                                sd.Device.ProtocolData["Address"] = read.ProtocolAddress.ToString();

                                devicesInfo.Add(sd);
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
                    foreach (ScannedDeviceInfo sd in devices)
                    {
                        SavedDevice savedDevice = new SavedDevice();

                        if(sd.Device.PhyData.TryGetValue("Guid", out object guidObj))
                            if (guidObj is string str)
                                savedDevice.Id = str;

                        if (sd.Device.PhyData.TryGetValue("Mac", out object macObj))
                            if (macObj is string str)
                                savedDevice.Mac = str;
                        
                        savedDevice.DeviceName = sd.Device.Name;
                        savedDevice.BluetoothType = (BluetoothType)sd.Device.PhyId;
                        savedDevice.Kind = (ushort)sd.Device.Kind;
                        savedDevice.ProtocolId = sd.Device.ProtocolId;

                        if (sd.Device.ProtocolData.TryGetValue("Address", out object protAddressObj))
                            if (protAddressObj is string str)
                                if (byte.TryParse(str, out byte addr))
                                    savedDevice.ProtocolAddress = addr;

                        string jsonString = JsonConvert.SerializeObject(
                            savedDevice, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }
    }
}