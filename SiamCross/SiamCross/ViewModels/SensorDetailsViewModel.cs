using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.ViewModels.MeasurementViewModels;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class SensorDetailsViewModel: BaseVM
    {
        public ICommand ShowUserConfigViewCommand { get; set; }
        public ICommand ShowFactoryConfigViewCommand { get; set; }
        public ICommand ShowSurveysViewCommand { get; set; }
        public ICommand ShowStateViewCommand { get; set; }
        public ICommand ShowDownloadsViewCommand { get; set; }

        ISensor _Sensor;
        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;

                ShowUserConfigViewCommand = _Sensor.ShowUserConfigViewCommand;
                ShowFactoryConfigViewCommand = _Sensor.ShowFactoryConfigViewCommand;
                ShowSurveysViewCommand = new AsyncCommand(ShowSurveysCollection
                    , (Func<object, bool>)null, null, false, false);

                ShowStateViewCommand = _Sensor.ShowStateViewCommand;
                ShowDownloadsViewCommand = _Sensor.ShowDownloadsViewCommand;
                ChangeNotify();
            }
        }

        public SensorDetailsViewModel()
        {

        }

        private async Task ShowSurveysCollection()
        {
            var type = typeof(SurveysCollectionnViewModel);

            var view = ViewFactoryService.Get(type) as SurveysCollectionnView;
            if (null == view)
            {
                view = new SurveysCollectionnView();
                ViewFactoryService.Register(type, view);
            }
            var ctx = new SurveysCollectionnViewModel()
            {
                Sensor = _Sensor,
            };
            view = ViewFactoryService.Get<SurveysCollectionnView>(type, ctx);
            await App.NavigationPage.Navigation.PushAsync(view);
        }


    }
}
