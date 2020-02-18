using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
using SiamCross.Models.Tools;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.IO;
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

        private static readonly object _lock = new object();

        public int SensorsCount => _sensors.Count;

        private SensorService()
        {
            _sensors = new List<ISensor>();
        }

        public static SensorService Instance { get => _instance.Value; }

        private List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public event Action<SensorData> SensorAdded;

        public event Action<SensorData> SensorDataChanged;

        public void SensorDataChangedHandler(SensorData data)
        {
            SensorDataChanged?.Invoke(data);
        }

        public void Initinalize()
        {
            lock (_lock)
            {
                var savedSensors = SensorsSaverService.Instance.ReadSavedSensors();

                _sensors.Clear();

                if (savedSensors == null) return;

                foreach (var sensor in savedSensors)
                {
                    var addebleSensor = SensorFactory.CreateSensor(sensor);
                    if (addebleSensor != null)
                    {
                        _sensors.Add(addebleSensor);

                        SensorAdded?.Invoke(addebleSensor.SensorData);
                    }
                }
            }
        }

        public void AddSensor(ScannedDeviceInfo deviceInfo)
        {
            lock (_lock)
            {
                foreach (var sensor in Sensors)
                {
                    if (sensor.SensorData.Name == deviceInfo.Name)
                    {
                        return;
                    }
                }

                var addebleSensor = SensorFactory.CreateSensor(deviceInfo);
                if (addebleSensor == null)
                {
                    return;
                }

                _sensors.Add(addebleSensor);

                SensorAdded?.Invoke(addebleSensor.SensorData);

                MessagingCenter.Send(this, "Refresh saved sensors",
                    _sensors.Select(s => s.ScannedDeviceInfo));
            }
        }

        public void DeleteSensor(int id)
        {
            lock (_lock)
            {
                var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
                if (sensor != null)
                {
                    //await sensor.BluetoothAdapter.Disconnect();
                    _sensors.Remove(sensor);
                    sensor.Dispose();
                }
                MessagingCenter.Send(this, "Refresh saved sensors",
                    _sensors.Select(s => s.ScannedDeviceInfo));
            }
        }

        public IFileManager AppCotainer { get; private set; }

        public async Task StartMeasurementOnSensor(int id, object parameters)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                await sensor.StartMeasurement(parameters);
            }
        }

        public async void MeasurementHandler(object measurementArgs)
        {
            int addbleId;
            switch (measurementArgs)
            {
                case Ddim2MeasurementData ddim2Data:
                    var dbModelDdim2 = new Ddim2Measurement(ddim2Data);
                    addbleId = DataRepository.Instance.SaveDdim2Measurement(dbModelDdim2);
                    await App.NavigationPage.Navigation.PushModalAsync(
                            new Ddim2MeasurementDonePage(
                                DataRepository.Instance.GetDdim2MeasurementById(addbleId)),
                                true);
                    break;
                case Ddin2MeasurementData ddin2Data:
                    var dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    addbleId = DataRepository.Instance.SaveDdin2Measurement(dbModelDdin2);
                    var dbObj = DataRepository.Instance.GetDdin2MeasurementById(addbleId);
                    await App.NavigationPage.Navigation.PushModalAsync(
                           new Ddin2MeasurementDonePage(dbObj), true);                    
                    break;
                case SiddosA3MMeasurementData siddosA3M:
                    var dbModelsiddosA3M = new SiddosA3MMeasurement(siddosA3M);
                    addbleId = DataRepository.Instance.SaveSiddosA3MMeasurement(dbModelsiddosA3M);
                    await App.NavigationPage.Navigation.PushModalAsync(
                            new SiddosA3MMeasurementDonePage(
                                DataRepository.Instance.GetSiddosA3MMeasurementById(addbleId)),
                                true);
                    break;
                default:
                    break;
            }
        }
    }
}
