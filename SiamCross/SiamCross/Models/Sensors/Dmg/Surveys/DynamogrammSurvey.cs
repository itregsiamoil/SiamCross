using SiamCross.ViewModels.Dmg.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    public class DynamogrammSurvey : BaseSurveyModel
    {
        public Kind SurveyType => Kind.Dynamogramm;

        public DynamogrammSurvey(SensorModel sensor)
            : base(sensor, null, Kind.Dynamogramm.Title(), Kind.Dynamogramm.Info())
        {
            Config = new DynamogrammSurveyCfg(sensor);
            //TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
        public override BaseSurveyVM GetCfgVM(ISensor sensorVM)
        {
            return new DynamogrammSurveyCfgVM(sensorVM, this);
        }

    }
}
