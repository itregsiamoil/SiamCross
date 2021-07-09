using System;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DynamogrammSurveyCfg : BaseSurveyCfgModel
    {
        public readonly SensorModel Sensor;

        public struct Data
        {
            public UInt32 Test;
        }
        public Data? Saved { get; private set; }
        public Data Current;

        public override void ResetSaved()
        {
            Saved = null;
        }
        public override void UpdateSaved()
        {
            if (null == Saved)
                Saved = new Data();
            Saved = Current;
        }
        public DynamogrammSurveyCfg(SensorModel sensor)
        {
            Sensor = sensor;
            //TaskLoad = new TaskSurveyCfgLoad(this);
            //TaskSave = new TaskSurveyCfgSave(this);
            //TaskWait = new TaskSurveyWait(sensor);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
