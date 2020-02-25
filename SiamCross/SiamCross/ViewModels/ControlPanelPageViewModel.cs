using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;
using System;
using SiamCross.Services.Logging;
using SiamCross.AppObjects;
using Autofac;
using NLog;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public ObservableCollection<SensorData> SensorsData { get; }

        public ControlPanelPageViewModel()
        {
            SensorsData = new ObservableCollection<SensorData>();

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDataChanged += SensorsDataChanged;
            SensorService.Instance.Initinalize();
        }

        public ICommand DeleteSensorCommand
        {
            get => new Command<Guid>(DeleteSensorHandler);
        }

        private void DeleteSensorHandler(Guid id)
        {
            try
            {
                var sensor = SensorService.Instance.Sensors
                    .SingleOrDefault(s => s.SensorData.Id == id);
                if (sensor != null)
                {
                    if (!sensor.IsMeasurement)
                    {
                        var sensorData = SensorsData.FirstOrDefault(s => s.Id == id);
                        if (sensorData != null)
                        {
                            SensorsData.Remove(sensorData);
                        }
                        SensorService.Instance.DeleteSensor(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler");
                throw;
            }
        }

        private void SensorAdded(SensorData sensorData)
        {
            if (!SensorsData.Contains(sensorData))
            {
                SensorsData.Add(sensorData);
            }
        }

        public void SensorsDataChanged(SensorData data)
        {
            
        }
    }
}
