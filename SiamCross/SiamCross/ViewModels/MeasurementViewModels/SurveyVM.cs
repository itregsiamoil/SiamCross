using SiamCross.Models.Sensors;
using SiamCross.Views;
using System;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class SurveyVM
    {
        public SurveyVM(ISensor sensor, string name, string description, Page cfgPage=null)
        {
            Sensor = sensor;
            Name = name;
            Description = description;
         
            ShowViewCommand = new AsyncCommand(
                () => App.NavigationPage.Navigation.PushAsync(new SurveyView(Sensor, this))
                , (Func<object, bool>)null, null, false, false);

            ShowConfigViewCommand = new AsyncCommand(
                () => App.NavigationPage.Navigation.PushAsync(cfgPage)
                , (Func<object, bool>)null, null, false, false);
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
