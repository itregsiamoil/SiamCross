using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services.Environment;
using SiamCross.Services.RepositoryTables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorsSaverService
    {
#if DEBUG
        private static readonly string SavedDevicesFilePath = Path.Combine(
            EnvironmentService.Instance.GetDir_Downloads(), "SavedDevices.xml");
#else
        private static readonly string SavedDevicesFilePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedDevices.xml");
#endif

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
        /*
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
        */
        public async Task<IEnumerable<ScannedDeviceInfo>> ReadSavedSensorsAsync()
        {
            List<ScannedDeviceInfo> sensors = await LoadDevices();

            return sensors;
        }
        public async Task SaveDevicesAsync(IEnumerable<ScannedDeviceInfo> devices)
        {
            using (await _lock.UseWaitAsync())
            {
                //SaveDevicesXML(devices);
                await SaveDevicesDb(devices);
            }
        }
        public async Task<List<ScannedDeviceInfo>> LoadDevices()
        {
            using (await _lock.UseWaitAsync())
            {
                //return LoadDevicesXML();
                return await LoadDevicesDb();
            }
        }

        private static long TryGetLong(Dictionary<string, object> arr, string key)
        {
            if (arr.TryGetValue(key, out object objVal) && objVal is long tval)
                return tval;
            return default;
        }
        private static string TryGetString(Dictionary<string, object> arr, string key)
        {
            if (arr.TryGetValue(key, out object objVal) && objVal is string tval)
                return tval;
            return default;
        }
        private async Task SaveDevicesDb(IEnumerable<ScannedDeviceInfo> devices)
        {
            try
            {
                int entytyId = 0;
                foreach (var item in devices)
                {
                    var cfg = new Dictionary<string, object>();

                    cfg.Add(nameof(item.Device.Kind), item.Device.Kind);
                    cfg.Add(nameof(item.Device.Number), item.Device.Number);
                    cfg.Add(nameof(item.Device.Name), item.Device.Name);
                    cfg.Add(nameof(item.Device.ProtocolId), item.Device.ProtocolId);
                    cfg.Add(nameof(item.Device.PhyId), item.Device.PhyId);

                    var cfgPhy = new Dictionary<string, object>();
                    foreach (var phy in item.Device.PhyData)
                        cfgPhy.Add($"{phy.Key}", phy.Value);

                    var cfgProto = new Dictionary<string, object>();
                    foreach (var pd in item.Device.ProtocolData)
                        cfgProto.Add($"{pd.Key}", pd.Value);

                    await Repo.Config.SetAsync(EntityKind.Device, entytyId, cfg);
                    await Repo.Config.SetAsync(EntityKind.DevicePhy, entytyId, cfgPhy);
                    await Repo.Config.SetAsync(EntityKind.DeviceProtocol, entytyId, cfgProto);
                    entytyId++;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION msg={ex.Message}\n{ex.StackTrace}");
            }
        }
        private async Task<List<ScannedDeviceInfo>> LoadDevicesDb()
        {
            var list = new List<ScannedDeviceInfo>();
            try
            {
                var loadDevice = await Repo.Config.GetAsync(EntityKind.Device);
                var loadPhy = await Repo.Config.GetAsync(EntityKind.DevicePhy);
                var loadProt = await Repo.Config.GetAsync(EntityKind.DeviceProtocol);
                long idx = 0;
                foreach (var d in loadDevice)
                {
                    var di = new DeviceInfo();
                    di.Kind = (uint)TryGetLong(d.Value, nameof(DeviceInfo.Kind));
                    di.Number = (uint)TryGetLong(d.Value, nameof(DeviceInfo.Number));
                    di.Name = TryGetString(d.Value, nameof(DeviceInfo.Name));
                    di.ProtocolId = (uint)TryGetLong(d.Value, nameof(DeviceInfo.ProtocolId));
                    di.PhyId = (uint)TryGetLong(d.Value, nameof(DeviceInfo.PhyId));

                    foreach (var phyAttr in loadPhy[idx])
                        di.PhyData.Add(phyAttr.Key, phyAttr.Value);

                    foreach (var protAttr in loadProt[idx])
                        di.ProtocolData.Add(protAttr.Key, protAttr.Value);

                    idx++;
                    list.Add(new ScannedDeviceInfo(di));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION msg={ex.Message}\n{ex.StackTrace}");
            }
            return list;
        }
    }
}
