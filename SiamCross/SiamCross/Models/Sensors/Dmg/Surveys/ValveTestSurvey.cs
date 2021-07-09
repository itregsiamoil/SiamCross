using SiamCross.ViewModels.Dmg.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    class ValveTestSurvey : BaseSurveyModel
    {
        public Kind SurveyType => Kind.ValveTest;

        public ValveTestSurvey(SensorModel sensor)
            : base(sensor, null, Kind.ValveTest.Title(), Kind.ValveTest.Info())
        {
            //Config = new DynamogrammSurveyCfg(sensor);
            //TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
        public override BaseSurveyVM GetCfgVM(ISensor sensorVM)
        {
            return new ValveTestSurveyCfgVM(sensorVM, this);
        }

    }
}
