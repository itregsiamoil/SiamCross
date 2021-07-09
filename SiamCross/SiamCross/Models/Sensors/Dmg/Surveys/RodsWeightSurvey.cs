using SiamCross.ViewModels.Dmg.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    class RodsWeightSurvey : BaseSurveyModel
    {
        public Kind SurveyType => Kind.RodWeight;

        public RodsWeightSurvey(SensorModel sensor)
            : base(sensor, null, Kind.RodWeight.Title(), Kind.RodWeight.Info())
        {
            //Config = new DynamogrammSurveyCfg(sensor);
            //TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
        public override BaseSurveyVM GetCfgVM(ISensor sensorVM)
        {
            return new RodsWeightSurveyCfgVM(sensorVM, this);
        }
    }
}
