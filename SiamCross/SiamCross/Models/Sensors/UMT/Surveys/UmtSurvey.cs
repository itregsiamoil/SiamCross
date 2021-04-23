using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Umt.Surveys
{
    public class UmtSurvey : BaseSurvey
    {
        public Kind SurveyType { get; }

        Task DoSurvey()
        {
            return Task.CompletedTask;
        }
        Task DoWaitSurvey()
        {
            return Task.CompletedTask;
        }
        public UmtSurvey(SensorModel sensor, ISurveyCfg cfg, Kind kind)
            : base(sensor, cfg, kind.Title(), kind.Info())
        {
            SurveyType = kind;

            CmdStart = new AsyncCommand(DoSurvey,
                () => _Sensor.Manager.IsFree,
                null, false, false);

            CmdWait = new AsyncCommand(DoWaitSurvey,
                () => _Sensor.Manager.IsFree,
                null, false, false);
        }
    }
}
