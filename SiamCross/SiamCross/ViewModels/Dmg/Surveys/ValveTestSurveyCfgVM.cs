using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.ViewModels.Dmg.Surveys
{
    class ValveTestSurveyCfgVM : BaseSurveyVM
    {
        public ValveTestSurveyCfgVM(ISensor sensor, BaseSurveyModel model)
            : base(sensor, model)
        {
        }
    }
}
