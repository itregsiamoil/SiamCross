using SiamCross.Models.Sensors;
using System.Windows.Input;

namespace SiamCross.ViewModels
{
    public class SensorDetailsVM : BaseVM
    {
        public ICommand ShowUserConfigViewCommand { get; set; }
        public ICommand ShowFactoryConfigViewCommand { get; set; }
        public ICommand ShowSurveysViewCommand { get; set; }
        public ICommand ShowStateViewCommand { get; set; }
        public ICommand ShowDownloadsViewCommand { get; set; }
        public ICommand ShowPositionEditorCommand { get; set; }
        public ICommand ShowInfoViewCommand { get; set; }

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

        public SensorDetailsVM(ISensor sensor)
        {
            _Sensor = sensor;
            ShowDownloadsViewCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.DownloaderVM);
            ShowUserConfigViewCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.UserConfigVM);
            ShowFactoryConfigViewCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.FactoryConfigVM);
            ShowStateViewCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.StateVM);
            ShowSurveysViewCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.SurveysVM);
            ShowPositionEditorCommand = BaseSensor.CreateAsyncCommand(() => _Sensor.PositionVM);
        }
    }//public class SensorDetailsViewModel : BaseVM
}
