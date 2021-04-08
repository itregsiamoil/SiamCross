using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class SurveyVM : BaseSurveyVM
    {
        public SurveyVM(ISensor sensor, ISurvey survey, string name, string description)
            : base(survey)
        {
            //Page cfgPage = null;
            Sensor = sensor;
            Name = name;
            Description = description;

            ShowConfigViewCommand = new AsyncCommand(Show
                , (Func<object, bool>)null, null, false, true);

        }
        private async Task Show()
        {
            var ctx = this;
            var view = PageNavigator.Get(ctx);
            await App.NavigationPage.Navigation.PushAsync(view);
            CmdUpdate.Execute(this);
        }
        public string Name { get; }
        public string Description { get; }

        public ISensor Sensor { get;  }

        public ICommand ShowConfigViewCommand { get; set; }
    }
}
