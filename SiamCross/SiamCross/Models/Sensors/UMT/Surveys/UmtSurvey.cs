namespace SiamCross.Models.Sensors.Umt.Surveys
{
    public class UmtSurvey : BaseSurvey
    {
        public Kind SurveyType { get; }

        public UmtSurvey(SensorModel sensor, ISurveyCfg cfg, Kind kind)
            : base(sensor, cfg, kind.Title(), kind.Info())
        {
            SurveyType = kind;
            TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
    }
}
