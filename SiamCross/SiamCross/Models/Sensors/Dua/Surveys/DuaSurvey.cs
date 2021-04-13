using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class DuaSurvey : BaseSurvey
    {
        public byte SurveyType { get; }

        async Task DoSurvey()
        {
            AsyncCommand cmdSaveParam = Config?.CmdSaveParam as AsyncCommand;
            await cmdSaveParam.ExecuteAsync();

            var manager = _Sensor.Model.Manager;
            
            
            
            string taskName = Name + "-измерение";
            var taskSurvey = new TaskSurvey(_Sensor, taskName, SurveyType);
            await manager.Execute(taskSurvey);

            var taskWaitSurvey = new TaskWaitSurvey(_Sensor);
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
