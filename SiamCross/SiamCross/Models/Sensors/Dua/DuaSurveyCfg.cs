using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSurveyCfg : BaseSurveyCfg
    {
        private readonly SensorModel _Sensor;

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
            var taskSaveInfo = new TaskSurveyCfgSave(this, _Sensor);
            await _Sensor.Manager.Execute(taskSaveInfo);
        }
        async Task DoLoad()
        {
            var taskUpdate = new TaskSurveyCfgLoad(this, _Sensor);
            await _Sensor.Manager.Execute(taskUpdate);
        }

        public DuaSurveyCfg(SensorModel sensor)
        {
            _Sensor = sensor;

            CmdLoadParam = new AsyncCommand(DoLoad,
                () => _Sensor.Manager.IsFree,
                null, false, false);

            CmdSaveParam = new AsyncCommand(DoSave,
                () => _Sensor.Manager.IsFree,
                null, false, false);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
