using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class DuaSurvey : BaseSurvey
    {
        public Kind SurveyType { get; }

        async Task DoSurvey()
        {
            var cmdSaveParam = Config?.CmdSaveParam as AsyncCommand;
            await cmdSaveParam.ExecuteAsync();

            var taskSurvey = new TaskSurvey(_Sensor, Name, SurveyType);
            await _Sensor.Manager.Execute(taskSurvey);

            await DoWaitSurvey();
        }
        async Task DoWaitSurvey()
        {
            var taskWaitSurvey = new TaskSurveyWait(_Sensor);
            await _Sensor.Manager.Execute(taskWaitSurvey);
        }



        public DuaSurvey(SensorModel sensor, ISurveyCfg cfg, Kind kind)
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
