using SiamCross.Models.Sensors;
using SiamCross.ViewModels;
using SiamCross.ViewModels.MeasurementViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SurveyView : ContentPage
    {
        public ISensor Sensor { get; private set; }
        public SurveyVM Survey { get; private set; }
        public PositionInfoVM Position => Sensor.Position;

        public SurveyView(ISensor sensor, SurveyVM surveyVM)
        {
            Sensor = sensor;
            Survey = surveyVM;
            BindingContext = this;
            InitializeComponent();
        }
    }
}