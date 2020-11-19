using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static SensorService Instance { get => _instance.Value; }

        private List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public event Action<ISensor> SensorAdded;

        public void Initinalize()
        {
            lock (_lock)
            {
                var savedSensors = SensorsSaverService.Instance.ReadSavedSensors();

                _sensors.Clear();

                if (savedSensors == null) return;

                foreach (var sensor in savedSensors)
                {
                    switch (sensor.BluetoothType)
                    {
                        case BluetoothType.Le:
                        {
                            var addebleSensor = SensorFactory.CreateSensor(sensor);
                            if (addebleSensor != null)
                            {
                                _sensors.Add(addebleSensor);

                                SensorAdded?.Invoke(addebleSensor);
                            }

                            break;
                        }
                    }
                }

                Thread initBt2 = new Thread(() =>
                {
                    Thread.Sleep(2500);
                    foreach (var sensor in savedSensors)
                    {
                        switch (sensor.BluetoothType)
                        {
                            case BluetoothType.Classic:
                            {
                                var addebleSensor = SensorFactory.CreateSensor(sensor);
                                if (addebleSensor != null)
                                {
                                    _sensors.Add(addebleSensor);

                                    SensorAdded?.Invoke(addebleSensor);
                                }

                                break;
                            }
                        }
                    }
                });
                initBt2.Start();
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

                SensorAdded?.Invoke(addebleSensor);

                MessagingCenter.Send(this, "Refresh saved sensors",
                    _sensors.Select(s => s.ScannedDeviceInfo));

                Debug.WriteLine("AddSensor");
            }
        }

        public void DeleteSensor(Guid id)
        {
            lock (_lock)
            {
                var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
                if (sensor != null)
                {
                    _sensors.Remove(sensor);
                    sensor.Dispose();
                }
                MessagingCenter.Send(this, "Refresh saved sensors",
                    _sensors.Select(s => s.ScannedDeviceInfo));
            }
        }

        public IFileManager AppCotainer { get; private set; }

        public async Task StartMeasurementOnSensor(Guid id, object parameters)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                await sensor.StartMeasurement(parameters);
            }
        }

        public async void MeasurementHandler(object measurementArgs)
        {
            int addId;
            switch (measurementArgs)
            {
                case Ddim2MeasurementData ddim2Data:
                    var dbModelDdim2 = new Ddim2Measurement(ddim2Data);
                    addId = DataRepository.Instance.SaveDdim2Measurement(dbModelDdim2);

                    await App.NavigationPage.Navigation.PushAsync(
                        new Ddim2MeasurementDonePage(
                            DataRepository.Instance.GetDdim2MeasurementById(addId)), true);
                    break;
                case Ddin2MeasurementData ddin2Data:
                    var dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    addId = DataRepository.Instance.SaveDdin2Measurement(dbModelDdin2);
                    var dbObj = DataRepository.Instance.GetDdin2MeasurementById(addId);

                    await App.NavigationPage.Navigation.PushAsync(
                           new Ddin2MeasurementDonePage(dbObj), true);                    
                    break;
                case SiddosA3MMeasurementData siddosA3M:
                    var dbModelsiddosA3M = new SiddosA3MMeasurement(siddosA3M);
                    addId = DataRepository.Instance.SaveSiddosA3MMeasurement(dbModelsiddosA3M);
                    await App.NavigationPage.Navigation.PushAsync(
                            new SiddosA3MMeasurementDonePage(
                                DataRepository.Instance.GetSiddosA3MMeasurementById(addId)),
                                true);
                    break;
                case DuMeasurementData duData:
                    var dbModelDu = new DuMeasurement(duData);
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
