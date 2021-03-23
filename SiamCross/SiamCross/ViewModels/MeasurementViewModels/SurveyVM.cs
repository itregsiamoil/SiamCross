using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels.MeasurementViewModels
{

    public class SurveysCollectionnViewModel : BaseVM
    {
        public ISensor Sensor { get; set; }
        public ObservableCollection<SurveyVM> SurveysCollection { get; set; }
    }

    public class SurveyViewModel : BaseVM
    {
        public ISensor Sensor { get; set; }
        public SurveyVM Survey { get; set; }
    }



    public class SurveyVM : BaseVM
    {
        private async Task Show()
        {
            var type = typeof(SurveyVM);
            var view = ViewFactoryService.Get(type) as SurveyView;
            if (null == view)
            {
                view = new SurveyView();
                ViewFactoryService.Register(type, view);
            }
            view.BindingContext = new SurveyViewModel()
            {
                Survey = this,
                Sensor = this.Sensor
            };
            await App.NavigationPage.Navigation.PushAsync(view);
        }


        private static AsyncCommand CreateCommand(Page cfgPage)
        {
            if (null == cfgPage)
                return null;
            return new AsyncCommand(
                () => App.NavigationPage.Navigation.PushAsync(cfgPage)
                , (Func<object, bool>)null, null, false, false);
        }
        public SurveyVM(ISensor sensor, string name, string description, Page cfgPage = null)
        {
            Sensor = sensor;
            Name = name;
            Description = description;


            ShowViewCommand = new AsyncCommand(Show
                , (Func<object, bool>)null, null, false, false);

            ShowConfigViewCommand = CreateCommand(cfgPage);

        }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ISensor Sensor { get; private set; }

        public ICommand ShowViewCommand { get; private set; }

        public ICommand ShowInfoViewCommand { get; set; }
        public ICommand ShowConfigViewCommand { get; set; }
        /*
        public ICommand ShowPositionViewCommand { get; set; }

        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }
        */
    }
}
