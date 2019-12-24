﻿using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;
using SiamCross.Views;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Sensors.Ddim2.Measurement;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel, IViewModel
    {
        public ObservableCollection<SensorData> SensorsData { get; }

        public ControlPanelPageViewModel(ISaveDevicesService saveDevicesService)
        {
            SensorsData = new ObservableCollection<SensorData>();
            foreach (var sensor in SensorService.Instance.Sensors)
            {
                SensorsData.Add(sensor.SensorData);
            }

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDataChanged += SensorsDataChanged;
            SensorService.Instance.SaveDevicesService = saveDevicesService;
            SensorService.Instance.LoadSavedDevices();

            
        }

        public ICommand DeleteSensorCommand
        {
            get => new Command<int>(DeleteSensorHandler);
        }

        private async void DeleteSensorHandler(int id)
        {
            var sensorData = SensorsData.FirstOrDefault(s => s.Id == id);
            if (sensorData != null)
            {
                SensorsData.Remove(sensorData);
            }
            await SensorService.Instance.DeleteSensor(id);
        }

        private void SensorAdded(SensorData sensorData)
        {
            SensorsData.Add(sensorData);
        }

        public void SensorsDataChanged(SensorData data)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //if (!data.FirstTime) return;

                //for (int i = 0; i < SensorsData.Count; i++)
                //{
                //    if (data.Id == SensorsData[i].Id)
                //    {
                //        SensorsData[i] = data;
                //        data.FirstTime = false;
                //    }
                //}

                ////  sensorData.Status = data.Status;
                //NotifyPropertyChanged(nameof(SensorsData));
            });
            
        }
    }
}
