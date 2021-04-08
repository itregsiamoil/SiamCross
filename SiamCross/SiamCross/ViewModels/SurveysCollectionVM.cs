using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class SurveysCollectionVM : BaseVM
    {
        public ISensor Sensor { get; }
        public ObservableCollection<SurveyVM> SurveysCollection { get; set; }

        public SurveysCollectionVM(ISensor sensor)
        {
            Sensor = sensor;
            SurveysCollection = new ObservableCollection<SurveyVM>();
            //Sensor.Model.Surveys.ForEach(o => SurveysCollection.Add(o));

        }
    }
}
