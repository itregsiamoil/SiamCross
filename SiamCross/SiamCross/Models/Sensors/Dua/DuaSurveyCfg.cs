using System;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSurveyCfg : BaseSurveyCfgModel
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
        public DateTime Timestamp;

        public DuaSurveyCfg(SensorModel sensor)
        {
            _Sensor = sensor;

            TaskSave = new TaskSurveyCfgSave(this, _Sensor);
            TaskLoad = new TaskSurveyCfgLoad(this, _Sensor);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
