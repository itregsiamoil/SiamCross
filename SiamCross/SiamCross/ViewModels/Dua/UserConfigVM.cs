using SiamCross.Models.Sensors;
using System.Windows.Input;

namespace SiamCross.ViewModels.Dua
{
    class UserConfigVM : BaseVM
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
        public UserConfigVM(ISensor sensor)
        {
            _Sensor = sensor;
        }
    }
}
