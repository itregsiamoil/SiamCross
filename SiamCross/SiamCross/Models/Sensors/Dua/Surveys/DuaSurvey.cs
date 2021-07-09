using SiamCross.ViewModels.Dua.Survey;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class DuaSurvey : BaseSurveyModel
    {
        public Kind SurveyType { get; }

        public DuaSurvey(SensorModel sensor, BaseSurveyCfgModel cfg, Kind kind)
            : base(sensor, cfg, kind.Title(), kind.Info())
        {
            SurveyType = kind;
            TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
        public override BaseSurveyVM GetCfgVM(ISensor sensorVM)
        {
            return new SurveyVM(sensorVM, this);
        }
    }


}
