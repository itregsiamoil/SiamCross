using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class DuaSurvey : BaseSurvey
    {
        public byte SurveyType { get; }

        async Task DoSurvey()
        {
            var manager = _Sensor.Model.Manager;

            var cmdSaveParam = Config?.CmdSaveParam as AsyncCommand;
            await cmdSaveParam.ExecuteAsync();

            var taskSurvey = new TaskSurvey(_Sensor.Model, Name, SurveyType);
            await manager.Execute(taskSurvey);

            var taskWaitSurvey = new TaskWaitSurvey(_Sensor.Model);
            await manager.Execute(taskWaitSurvey);
        }

        public DuaSurvey(ISensor sensor, ISurveyCfg cfg, string name, string description, byte type)
            : base(sensor, cfg, name, description)
        {
            SurveyType = type;

            var manager = _Sensor.Model.Manager;

            CmdStart = new AsyncCommand(DoSurvey,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
        }
    }


}
