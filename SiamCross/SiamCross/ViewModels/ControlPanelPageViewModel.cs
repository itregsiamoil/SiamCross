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
            foreach (var sensor in SensorService.Instance.Sensors)
            {
                SensorsData.Add(sensor.SensorData);
            }

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDataChanged += SensorsDataChanged;
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
