using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels
{
    public class SensorDetailsVM : BasePageVM
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
            ShowDownloadsViewCommand = new AsyncCommand(ShowStoragePage
                , (Func<object, bool>)null, null, false, true);

            ShowPositionEditorCommand = new AsyncCommand(ShowPositionPage
                , (Func<object, bool>)null, null, false, true);

            ShowSurveysViewCommand = new AsyncCommand(ShowSurveysPage
                , (Func<object, bool>)null, null, false, true);

            //ShowUserConfigViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.UserConfigVM);
            //ShowFactoryConfigViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.FactoryConfigVM);
            //ShowStateViewCommand = PageNavigator.CreateAsyncCommand(() => _Sensor.StateVM);
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        Task ShowSurveysPage()
        {
            return PageNavigator.ShowPageAsync(_Sensor.SurveysVM);
        }
        async Task ShowPositionPage()
        {
            await PageNavigator.ShowPageAsync(_Sensor.PositionVM);
            await Sensor.Model.Manager.Execute(Sensor.Model.Position.TaskLoad);
        }
        async Task ShowStoragePage()
        {
            _Sensor.Model.Storage?.CmdUpdateStorageInfo?.Execute(this);
            await PageNavigator.ShowPageAsync(_Sensor.StorageVM);
        }

    }//public class SensorDetailsViewModel : BaseVM
}
