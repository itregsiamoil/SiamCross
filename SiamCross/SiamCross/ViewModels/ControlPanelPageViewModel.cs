using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel, IViewModel
    {
        public ObservableCollection<SensorData> SensorsData { get; }

        public ControlPanelPageViewModel()
        {
            SensorsData = new ObservableCollection<SensorData>();
            foreach (var sensor in SensorsService.Instance.Sensors)
            {
                SensorsData.Add(sensor.SensorData);
            }
        }

        public void SensorsDataChanged(SensorData data)
        {
            foreach (var sensorData in SensorsData)
            {
                if(sensorData.Id == data.Id)
                {
                    sensorData.Status = data.Status;
                    NotifyPropertyChanged(nameof(SensorsData));
                }
            }
        }
    }
}
