using SiamCross.Models.Connection.Protocol;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class Level : BaseSurvey
    {
        private readonly ISensor _Sensor;
        private readonly IProtocolConnection _Connection;

        public bool IsValveAutomaticEnabled;
        public bool IsValveDurationShort;
        public bool IsValveDirectionInput;
        public bool IsPiezoDepthMax;
        public bool IsPiezoAdditionalGain;
        public double SoundSpeedFixed = DefaultSoundSpeedFixed;
        public ushort SoundSpeedTableId;

        public static readonly double DefaultSoundSpeedFixed = 320;

        public Level(ISensor sensor, byte levelMode, string name, string description)
        {
            Name = name;
            Description = description;

            _Sensor = sensor;
            _Connection = sensor.Connection;

            var manager = _Sensor.Model.Manager;
            var taskUpdate = new TaskUpdateLevelInfo(this, _Sensor);
            CmdUpdate = new AsyncCommand(
                () => manager.Execute(taskUpdate),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);

            string taskName = Name + "-измерение";

            var taskSurvey = new TaskSurveyLevel(this, _Sensor, taskName, levelMode);
            CmdStart = new AsyncCommand(
                () => manager.Execute(taskSurvey),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
        }
    }
}
