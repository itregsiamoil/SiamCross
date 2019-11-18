using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel
    {
        public ObservableCollection<SensorData> SensorsData { get; }

        public ControlPanelPageViewModel()
        {
            SensorsData = new ObservableCollection<SensorData>();
            foreach (var sensor in SensorService.Instance.Sensors)
            {
                SensorsData.Add(sensor.SensorData);
            }
        }
    }
}
