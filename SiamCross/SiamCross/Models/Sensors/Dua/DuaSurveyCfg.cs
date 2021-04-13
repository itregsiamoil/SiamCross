using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSurveyCfg : BaseSurveyCfg
    {
        private readonly ISensor _Sensor;

        public bool Synched = false;

        public bool IsAutoswitchToAPR;
        public bool IsValveAutomaticEnabled;
        public bool IsValveDurationShort;
        public bool IsValveDirectionInput;
        public bool IsPiezoDepthMax;
        public bool IsPiezoAdditionalGain;
        public double SoundSpeedFixed = Constants.DefaultSoundSpeedFixed;
        public ushort SoundSpeedTableId;

        public byte PressurePeriodIndex;
        public byte PressureQuantityIndex;
        public readonly byte[] LevelPeriodIndex = new byte[5];
        public readonly byte[] LevelQuantityIndex = new byte[5];

        async Task DoSave()
        {
            var manager = _Sensor.Model.Manager;
            var taskSaveInfo = new TaskSaveSurveyInfo(this, _Sensor);
            await manager.Execute(taskSaveInfo);
        }
        async Task DoLoad()
        {
            var manager = _Sensor.Model.Manager;
            var taskUpdate = new TaskLoadSurveyInfo(this, _Sensor);
            await manager.Execute(taskUpdate);
        }

        public DuaSurveyCfg(ISensor sensor)
        {
            _Sensor = sensor;

            var manager = _Sensor.Model.Manager;

            CmdLoadParam = new AsyncCommand(DoLoad,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);

            CmdSaveParam = new AsyncCommand(DoSave,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
