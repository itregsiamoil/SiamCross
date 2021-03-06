﻿using System;
using System.Collections.Specialized;

namespace SiamCross.Models.Sensors.Umt
{
    public class SurveyCfg : BaseSurveyCfgModel
    {
        public readonly SensorModel Sensor;

        public DateTime Timestamp;
        public struct Data
        {
            public UInt32 Interval;
            public UInt16 Revbit;
            public bool IsExtetnalTemp;
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
        public override bool IsSync() { return Saved.HasValue; }

        public UInt32 Period
        {
            get => Current.Interval / 10000;
            set => SetProperty(ref Current.Interval, value * 10000);
        }
        public bool IsEnabledTempRecord
        {
            get => 0 == (Current.Revbit & (1 << 1));
            set
            {
                BitVector32 myBV = new BitVector32(Current.Revbit);
                int bit0 = BitVector32.CreateMask();
                int bit1 = BitVector32.CreateMask(bit0);
                myBV[bit1] = !value;
                Current.Revbit = (ushort)myBV.Data;
                ChangeNotify();
            }
        }
        public bool IsEnabledExtTemp => Current.IsExtetnalTemp;


        public SurveyCfg(SensorModel sensor)
        {
            Sensor = sensor;
            TaskLoad = new TaskSurveyCfgLoad(this);
            TaskSave = new TaskSurveyCfgSave(this);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
