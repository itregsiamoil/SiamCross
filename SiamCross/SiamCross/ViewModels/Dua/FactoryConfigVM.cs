using SiamCross.Models.Sensors;
using System.Windows.Input;

namespace SiamCross.ViewModels.Dua
{
    class FactoryConfigVM : BaseVM
    {
        ISensor _Sensor;
        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;
                ChangeNotify();
            }
        }
        public ICommand LoadFromDeviceCommand { get; set; }
        public FactoryConfigVM(ISensor sensor)
        {
            _Sensor = sensor;
        }
    }
}
