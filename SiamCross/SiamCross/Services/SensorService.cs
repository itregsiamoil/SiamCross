﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddim2.Measurement;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Views;

namespace SiamCross.Services
{
    public sealed class SensorService 
    {
        private static readonly Lazy<SensorService> _instance =
            new Lazy<SensorService>(() => new SensorService());

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

        public void AddSensor(ScannedDeviceInfo deviceInfo)
        {
            foreach(var sensor in Sensors)
            {
                if(sensor.SensorData.Name == deviceInfo.Name)
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

            new Thread(async () =>
            {
                await SaveDevicesService.SaveDevices(
                    _sensors.Select(s => s?.ScannedDeviceInfo));
            }).Start();
        }


        public async Task DeleteSensor(int id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                //await sensor.BluetoothAdapter.Disconnect();
                _sensors.Remove(sensor);
                sensor.Dispose();
            }
            new Thread(async () =>
            {
                await SaveDevicesService.SaveDevices(
                    _sensors.Select(s => s?.ScannedDeviceInfo));
            }).Start();
        }

        public ISaveDevicesService SaveDevicesService { get; set; }

        public void LoadSavedDevices()
        {
            if (SaveDevicesService == null) return;

            var thread = new Thread(async () => 
            {
                var savedDevices = await SaveDevicesService.LoadDevices();

                foreach (var device in savedDevices)
                {
                    var addebleSensor = SensorFactory.CreateSensor(device);
                    if (addebleSensor == null)
                    {
                        return;
                    }
                    _sensors.Add(addebleSensor);
                    SensorAdded?.Invoke(addebleSensor.SensorData);
                }
            });
            thread.Start();
        }

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
            switch (measurementArgs)
            {
                case Ddim2MeasurementData ddim2Data:
                    var dbModelDdim2 = new Ddim2Measurement(ddim2Data);
                    DataRepository.Instance.SaveDdim2Item(dbModelDdim2);

                    await App.NavigationPage.Navigation.PushModalAsync(
                            new Ddim2MeasurementDonePage(
                                DataRepository.Instance.GetDdim2Item(dbModelDdim2.Id)), 
                                true);
                    break;
                case Ddin2MeasurementData ddin2Data:
                    var dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    DataRepository.Instance.SaveDdin2Item(dbModelDdin2);
                    await App.NavigationPage.Navigation.PushModalAsync(
                           new Ddin2MeasurementDonePage(
                               DataRepository.Instance.GetDdin2Item(dbModelDdin2.Id)),
                               true);
                    break;
                default:
                    break;
            }
        }
    }
}
