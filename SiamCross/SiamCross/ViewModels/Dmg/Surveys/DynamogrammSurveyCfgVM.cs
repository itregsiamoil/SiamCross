using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.ViewModels.Dmg.Surveys
{
    public class DynamogrammSurveyCfgVM : BaseSurveyVM
    {
        public DynamogrammSurveyCfgVM(ISensor sensorVM, DynamogrammSurvey model)
            : base(sensorVM, model)
        {
        }
    }
}
