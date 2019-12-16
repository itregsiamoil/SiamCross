using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class Ddin2MeasurementViewModel : BaseViewModel, IViewModel
    {
        private SensorData _sensorData;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comments { get; set; }
        public string Rod { get; set; }
        public string DynPeriod { get; set; }
        public string ApertNumber { get; set; }
        public string Imtravel { get; set; }
        public ObservableCollection<string> ModelPump { get; set; }
        public string SelectedModelPump { get; set; }
        public ICommand StartMeasurementCommand { get; set; }

        public Ddin2MeasurementViewModel(SensorData sensorData)
        {
            _sensorData = sensorData;
            SensorName = _sensorData.Name;
            Fields = new ObservableCollection<string>()
            {
                "Первое поле",
                "Второе поле",
                "Третье поле"
            };
            ModelPump = new ObservableCollection<string>()
            {
                "Балансирный",
                "Цепной",
                "Гидравлический"
            };
            StartMeasurementCommand = new Command(() =>
            {
                Console.WriteLine(Bush);
                Console.WriteLine(Shop);
                Console.WriteLine(BufferPressure);
                Console.WriteLine(Comments);
                Console.WriteLine(Rod);
                Console.WriteLine(DynPeriod);
                Console.WriteLine(ApertNumber);
                Console.WriteLine(Imtravel);
                Console.WriteLine(SelectedField);
                Console.WriteLine(SelectedModelPump);
            });
        }

        public string SensorName 
        { 
            get; 
            set; 
        }
    }
}
