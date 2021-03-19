using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SurveysCollectionnView : ContentPage
    {
        public ISensor Sensor { get; set; }
        public ObservableCollection<SurveyVM> SurveysCollection { get; set; }

        public SurveysCollectionnView(ISensor sensor)
        {
            Sensor = sensor;
            BindingContext = this;

            SurveysCollection = new ObservableCollection<SurveyVM>();
            Sensor.Surveys.ForEach(o => SurveysCollection.Add(o));
            InitializeComponent();


        }

        public SurveysCollectionnView()
        {
            InitializeComponent();
        }

    }
}
