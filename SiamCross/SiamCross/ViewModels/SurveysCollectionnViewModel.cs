using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class SurveysCollectionnViewModel : BaseVM
    {
        ISensor _Sensor;
        public ObservableCollection<SurveyVM> SurveysCollection { get; set; }
        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;
                SurveysCollection = new ObservableCollection<SurveyVM>();
                Sensor.Surveys.ForEach(o => SurveysCollection.Add(o));
                ChangeNotify();
            }
        }
        public SurveysCollectionnViewModel()
        {

        }
    }
}
