﻿using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ControlPanelPageViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public ObservableCollection<ISensor> Sensor { get; }
        public bool IsRelease => !Version.ToLower().Contains("rc");
        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
        public ControlPanelPageViewModel()
        {
            Sensor = new ObservableCollection<ISensor>();

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDeleting += SensorDeleted;
            Task.Run(() => SensorService.Instance.InitinalizeAsync());
        }
        public ICommand RecentMeasurementCommand => new Command<Guid>(RecentMeasurement);
        public ICommand DeleteSensorCommand => new Command<Guid>(DeleteSensorHandler);
        public ICommand GotoMeasurementPageCommand => new Command<Guid>(GotoMeasurementPage);
        private void RecentMeasurement(Guid id)
        {
            try
            {
                ISensor sensor = SensorService.Instance.Sensors
                    .SingleOrDefault(s => s.SensorData.Id == id);
                if (sensor != null)
                {
                    if (CanOpenPage(typeof(MeasurementsPage)))
                    {
                        App.NavigationPage.Navigation.PushAsync(new MeasurementsPage());
                        App.MenuIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");
                throw;
            }
        }
        private async void DeleteSensorHandler(Guid id)
        {
            try
            {
                ISensor sensor = SensorService.Instance.Sensors
                    .SingleOrDefault(s => s.SensorData.Id == id);
                if (sensor != null)
                {
                    if (!sensor.IsMeasurement)
                    {
                        await SensorService.Instance.DeleteSensorAsync(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");
                throw;
            }
        }
        private void SensorDeleted(ISensor sensor)
        {
            try
            {
                Sensor.Remove(sensor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");
                throw;
            }
        }
        private void SensorAdded(ISensor sensor)
        {

            Debug.WriteLine("SensorAdded in view");
            if (!Sensor.Contains(sensor))
            {
                Sensor.Add(sensor);
            }
        }

        private void GotoMeasurementPage(Guid id)
        {
            ISensor sensor = SensorService.Instance.Sensors
                .SingleOrDefault(s => s.SensorData.Id == id);
            if (sensor == null)
                return;
            SensorData sensorData = sensor.SensorData as SensorData;
            if (sensorData == null)
                return;
            if (!CanOpenMeasurement(sensorData))
                return;

            switch (sensor.ScannedDeviceInfo.Device.Kind)
            {
                case 0x1301:
                case 0x1302:
                case 0x1303:
                case 0x1401:
                case 0x1402:
                case 0x1403:
                    if (!CanOpenPage(typeof(Ddin2MeasurementPage)))
                        return;
                    App.NavigationPage.Navigation.PushAsync(
                        new Ddin2MeasurementPage(sensorData));
                    break;
                case 0x1101:
                    if (!CanOpenPage(typeof(DuMeasurementPage)))
                        return;
                    App.NavigationPage.Navigation.PushAsync(
                        new DuMeasurementPage(sensorData));
                    break;
                default: break;
            }

        }
        private bool CanOpenMeasurement(SensorData sensorData)
        {
            bool result = true;
            ISensor sensor = SensorService.Instance.Sensors.SingleOrDefault(
                                s => s.SensorData.Id == sensorData.Id);
            if (sensor != null)
            {
                if (sensor.IsAlive)
                {
                    if (sensor.IsMeasurement)
                        result = false;
                }
                else result = false;
            }

            return result;
        }
        private bool CanOpenModalPage(Type type)
        {
            bool result = false;
            System.Collections.Generic.IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;

            if (stack.Count > 0)
            {
                if (stack[stack.Count - 1].GetType() != type)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
        private bool CanOpenPage(Type type)
        {
            try
            {
                System.Collections.Generic.IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
                if (stack[stack.Count - 1].GetType() != type)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanOpenPage method" + "\n");
                throw;
            }
        }
    }//public class ControlPanelPageViewModel : BaseViewModel, IViewModel
}
