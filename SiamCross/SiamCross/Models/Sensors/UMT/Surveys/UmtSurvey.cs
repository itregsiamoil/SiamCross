using SiamCross.ViewModels.MeasurementViewModels;
using SiamCross.ViewModels.Umt;

namespace SiamCross.Models.Sensors.Umt.Surveys
{
    public class UmtSurvey : BaseSurveyModel
    {
        public Kind SurveyType { get; }

        public UmtSurvey(SensorModel sensor, BaseSurveyCfgModel cfg, Kind kind)
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
