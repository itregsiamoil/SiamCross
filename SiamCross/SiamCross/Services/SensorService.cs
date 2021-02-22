using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorService
    {
        private static readonly Lazy<SensorService> _instance =
            new Lazy<SensorService>(() => new SensorService());

        private static readonly object _lock = new object();

        public int SensorsCount
        {
            get
            {
                lock (_lock)
                {
                    return _sensors.Count;
                }
            }
        }

        private SensorService()
        {
            _sensors = new List<ISensor>();
        }

        public static SensorService Instance => _instance.Value;

        private readonly List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public event Action<ISensor> SensorAdded;
        public event Action<ISensor> SensorDeleting;

        public void Initinalize()
        {
            lock (_lock)
            {
                IEnumerable<ScannedDeviceInfo> savedSensors = SensorsSaverService.Instance.ReadSavedSensors();

                _sensors.Clear();

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

        public void AddSensor(ScannedDeviceInfo deviceInfo)
        {
            lock (_lock)
            {
                foreach (ISensor sensor in Sensors)
                {
                    if (sensor.SensorData.Name == deviceInfo.Name)
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
        public void DeleteSensor(Guid id)
        {
            ISensor sensor;
            lock (_lock)
            {
                sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
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
            ISensor sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                await sensor.StartMeasurement(parameters);
            }
        }

        static public async void MeasurementHandler(object measurementArgs)
        {
            int addId;
            switch (measurementArgs)
            {
                case Ddin2MeasurementData ddin2Data:
                    Ddin2Measurement dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    addId = DataRepository.Instance.SaveDdin2Measurement(dbModelDdin2);
                    Ddin2Measurement dbObj = DataRepository.Instance.GetDdin2MeasurementById(addId);

                    await App.NavigationPage.Navigation.PushAsync(
                           new Ddin2MeasurementDonePage(dbObj), true);
                    break;
                case DuMeasurementData duData:
                    DuMeasurement dbModelDu = new DuMeasurement(duData);
                    addId = DataRepository.Instance.SaveDuMeasurement(dbModelDu);
                    await App.NavigationPage.Navigation.PushAsync(
                        new DuMeasurementDonePage(DataRepository.Instance.GetDuMeasurementById(addId)),
                        true);
                    break;
                default:
                    break;
            }
        }
    }
}
