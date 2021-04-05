using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

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
            ShowDownloadsViewCommand = new AsyncCommand(ShowDownloads
                , (Func<object, bool>)null, null, false, true);

            ShowUserConfigViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.UserConfigVM);
            ShowFactoryConfigViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.FactoryConfigVM);
            ShowStateViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.StateVM);
            ShowSurveysViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.SurveysVM);
            ShowPositionEditorCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.PositionVM);
        }

        private async Task ShowDownloads()
        {
            var ctx = _Sensor.DownloaderVM;
            var view = PageNavigator.Get(ctx);
            await App.NavigationPage.Navigation.PushAsync(view);
            var mgr = Sensor.TaskManager.GetModel();

            if (ctx is BaseMeasurementsDownloaderVM dvm)
                dvm.LoadFromDeviceCommand.Execute(this);
        }

    }//public class SensorDetailsViewModel : BaseVM
}
