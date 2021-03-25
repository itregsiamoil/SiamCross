using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class SurveysCollectionVM : BaseVM
    {
        ISensor _Sensor;
        public ObservableCollection<SurveyVM> SurveysCollection { get; set; }
        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;
                ChangeNotify();
            }
        }
        public SurveysCollectionVM(ISensor sensor)
        {
            _Sensor = sensor;
            SurveysCollection = new ObservableCollection<SurveyVM>();
            Sensor.Surveys.ForEach(o => SurveysCollection.Add(o));

        }
    }
}
