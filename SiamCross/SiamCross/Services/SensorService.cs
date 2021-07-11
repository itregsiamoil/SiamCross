using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorService
    {
        private static readonly Lazy<SensorService> _instance =
            new Lazy<SensorService>(() => new SensorService());

        private readonly SemaphoreSlim _lockAsync = new SemaphoreSlim(1);

        public int SensorsCount => _sensors.Count;
        private readonly List<ISensor> _sensors = new List<ISensor>();
        private SensorService()
        {
        }

        public static SensorService Instance => _instance.Value;


        public IEnumerable<ISensor> Sensors => _sensors;

        public event Action<ISensor> SensorAdded;
        public event Action<ISensor> SensorDeleting;

        private void Clear()
        {
            foreach (var s in _sensors)
            {
                SensorDeleting(s);
                s?.Dispose();
            }
            _sensors.Clear();
        }
        public async Task InitinalizeAsync()
        {
            using (await _lockAsync.UseWaitAsync())
            {
                Clear();

                IEnumerable<ScannedDeviceInfo> savedSensors = await SensorsSaverService.Instance.ReadSavedSensorsAsync();

                if (savedSensors == null) return;

                foreach (ScannedDeviceInfo sensor_info in savedSensors)
                {
                    ISensor sensor = SensorFactory.CreateSensor(sensor_info);
                    if (sensor == null)
                        continue;
                    _sensors.Add(sensor);
                    SensorAdded?.Invoke(sensor);
                }
            }
        }

        public async Task AddSensorAsync(ScannedDeviceInfo deviceInfo)
        {
            using (await _lockAsync.UseWaitAsync())
            {
                foreach (ISensor sensor in Sensors)
                {
                    if (sensor.Name == deviceInfo.Title)
                    {
                        return;
                    }
                }

                ISensor addebleSensor = SensorFactory.CreateSensor(deviceInfo);
                if (addebleSensor == null)
                {
                    return;
                }

                _sensors.Add(addebleSensor);

                SensorAdded?.Invoke(addebleSensor);

                MessagingCenter.Send(this, "Refresh saved sensors",
                    _sensors.Select(s => s.ScannedDeviceInfo));

                Debug.WriteLine("AddSensor");
            }
        }
        public async Task DeleteSensorAsync(Guid id)
        {
            ISensor sensor;
            using (await _lockAsync.UseWaitAsync())
            {
                sensor = _sensors.FirstOrDefault(s => s.Id == id);
                if (sensor == null)
                    return;
                SensorDeleting(sensor);
                _sensors.Remove(sensor);
            }
            MessagingCenter.Send(this, "Refresh saved sensors",
                _sensors.Select(s => s.ScannedDeviceInfo));
            sensor?.Dispose();
        }
        public async Task StartMeasurementOnSensor(Guid id, object parameters)
        {
            ISensor sensor = _sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null)
            {
                await sensor.StartMeasurement(parameters);
            }
        }

        public static async Task MeasurementHandler(object measurementArgs, bool show = true)
        {
            int addId;
            switch (measurementArgs)
            {
                case Ddin2MeasurementData ddin2Data:
                    Ddin2Measurement dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    addId = DbService.Instance.SaveDdin2Measurement(dbModelDdin2);
                    Ddin2Measurement dbObj = DbService.Instance.GetDdin2MeasurementById(addId);

                    if (show)
                        await App.NavigationPage.Navigation.PushAsync(
                           new Ddin2MeasurementDonePage(dbObj), true);
                    break;
                case DuMeasurementData duData:
                    DuMeasurement dbModelDu = new DuMeasurement(duData);
                    addId = DbService.Instance.SaveDuMeasurement(dbModelDu);

                    if (show)
                        await App.NavigationPage.Navigation.PushAsync(
                            new DuMeasurementDonePage(
                                await DbService.Instance.GetDuMeasurementByIdAsync(addId)), true);
                    break;
                default:
                    break;
            }
        }
    }
}
