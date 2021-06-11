using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;
using System.Collections.ObjectModel;

namespace SiamCross.ViewModels
{
    public class SurveysCollectionVM : BasePageVM
    {
        public ISensor Sensor { get; }
        public ObservableCollection<BaseSurveyVM> SurveysCollection { get; set; }

        public SurveysCollectionVM(ISensor sensor)
        {
            Sensor = sensor;
            SurveysCollection = new ObservableCollection<BaseSurveyVM>();
            //Sensor.Model.Surveys.ForEach(o => SurveysCollection.Add(o));

        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }
        public override void Dispose()
        {
            base.Dispose();
            foreach (var s in SurveysCollection)
                s.Dispose();
        }
    }
}
