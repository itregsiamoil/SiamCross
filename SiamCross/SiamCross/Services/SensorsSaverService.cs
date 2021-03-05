using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorsSaverService
    {
        private static readonly string SavedDevicesFilePath = Path.Combine(
            EnvironmentService.Instance.GetDir_Downloads(), "SavedDevices.xml");
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        private static readonly Lazy<SensorsSaverService> _instance =
            new Lazy<SensorsSaverService>(() => new SensorsSaverService());
        public static SensorsSaverService Instance => _instance.Value;
        private SensorsSaverService()
        {
            MessagingCenter.Subscribe<SensorService, IEnumerable<ScannedDeviceInfo>>(this,
                "Refresh saved sensors",
                async (sender, arg) =>
                {
                    await SaveDevicesAsync(arg);
                });
        }
        private List<ScannedDeviceInfo> LoadDevicesXML()
        {
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<DeviceInfo>));

                using (FileStream fs = new FileStream(SavedDevicesFilePath, FileMode.Open))
                {
                    List<DeviceInfo> dvc_list = (List<DeviceInfo>)formatter.Deserialize(fs);

                    List<ScannedDeviceInfo> dvcViewList = new List<ScannedDeviceInfo>();
                    foreach (var dvcInfo in dvc_list)
                    {
                        dvcViewList.Add(new ScannedDeviceInfo(dvcInfo));
                    }
                    return dvcViewList;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unknown EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return new List<ScannedDeviceInfo>();
        }
        private void SaveDevicesXML(IEnumerable<ScannedDeviceInfo> devices)
        {
            try
            {
                List<DeviceInfo> dvc_list = new List<DeviceInfo>();
                foreach (var item in devices)
                {
                    dvc_list.Add(item.Device);
                }

                XmlSerializer formatter = new XmlSerializer(typeof(DeviceInfo[]));

                using (FileStream fs = new FileStream(SavedDevicesFilePath, FileMode.Create))
                {
                    formatter.Serialize(fs, dvc_list.ToArray());
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unknown EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {

            }
        }
        public async Task<IEnumerable<ScannedDeviceInfo>> ReadSavedSensorsAsync()
        {
            List<ScannedDeviceInfo> sensors = await LoadDevices();

            return sensors;
        }
        public async Task SaveDevicesAsync(IEnumerable<ScannedDeviceInfo> devices)
        {
            using (await _lock.UseWaitAsync())
                SaveDevicesXML(devices);
        }
        public async Task<List<ScannedDeviceInfo>> LoadDevices()
        {
            using (await _lock.UseWaitAsync())
                return LoadDevicesXML();
        }

    }
}
