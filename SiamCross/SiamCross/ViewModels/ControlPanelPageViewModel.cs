using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel, IViewModel
    {
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
            get => new Command<int>(DeleteSensorHandler);
        }

        private void DeleteSensorHandler(int id)
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
