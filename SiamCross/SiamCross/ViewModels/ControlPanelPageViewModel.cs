using SiamCross.Models;
using SiamCross.Services;
using System.Collections.ObjectModel;
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
        }

        private void SensorAdded(SensorData sensorData)
        {
            SensorsData.Add(sensorData);
        }

        public void SensorsDataChanged(SensorData data)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //SensorData sens = null;
                //foreach (var sensorData in SensorsData)
                //{
                //    if (sensorData.Id == data.Id)
                //    {
                //        sens = sensorData;

                //    }
                //}
                //if (sens != null)
                //{
                //    SensorsData.Remove(sens);
                //    SensorsData.Add(data);
                //}

                //  sensorData.Status = data.Status;
                //NotifyPropertyChanged(nameof(SensorsData));

                //for (int i = 0; i < SensorsData.Count; i++)
                //{
                //    if (data.Id == SensorsData[i].Id)
                //    {
                //        SensorsData[i] = data;
                //        NotifyPropertyChanged(nameof(SensorsData));
                //    }
                //}
            });
            
        }
    }
}
