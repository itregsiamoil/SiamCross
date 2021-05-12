namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class DuaSurvey : BaseSurvey
    {
        public Kind SurveyType { get; }

        public DuaSurvey(SensorModel sensor, ISurveyCfg cfg, Kind kind)
            : base(sensor, cfg, kind.Title(), kind.Info())
        {
            SurveyType = kind;
            TaskStart = new TaskSurvey(_Sensor, Name, SurveyType);
        }
    }


}
