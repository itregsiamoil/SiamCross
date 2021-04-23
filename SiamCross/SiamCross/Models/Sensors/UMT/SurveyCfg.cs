using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Umt
{
    public class SurveyCfg : BaseSurveyCfg
    {
        public readonly SensorModel Sensor;
        public ITask TaskLoad { get; }
        public ITask TaskSave { get; }

        public struct Data
        {
            public UInt32 Interval;
            public UInt16 Revbit;
        }
        public Data? Saved { get; private set; }
        public Data Current;

        public override void ResetSaved()
        {
            Saved = null;
        }

        public UInt32 Period
        {
            get => Current.Interval / 10000;
            set => SetProperty(ref Current.Interval, value * 10000);
        }
        public bool IsEnabledTempRecord
        {
            get => 0 < (Current.Revbit & (1 << 1));
            set
            {
                BitVector32 myBV = new BitVector32(Current.Revbit);
                int bit0 = BitVector32.CreateMask();
                int bit1 = BitVector32.CreateMask(bit0);
                myBV[bit1] = value;
                Current.Revbit = (ushort)myBV.Data;
                ChangeNotify();
            }
        }

        public SurveyCfg(SensorModel sensor)
        {
            Sensor = sensor;
            TaskLoad = new TaskSurveyCfgLoad(this);
            TaskSave = new TaskSurveyCfgSave(this);

            CmdLoadParam = new AsyncCommand(DoLoad,
                () => Sensor.Manager.IsFree,
                null, false, false);

            CmdSaveParam = new AsyncCommand(DoSave,
                () => Sensor.Manager.IsFree,
                null, false, false);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
        async Task DoSave()
        {
            if (JobStatus.Сomplete == await Sensor.Manager.Execute(TaskSave))
                UpdateSaved();
            else
                ResetSaved();
        }
        async Task DoLoad()
        {
            if (JobStatus.Сomplete == await Sensor.Manager.Execute(TaskLoad))
                UpdateSaved();
            ChangeNotify(nameof(Period));
            ChangeNotify(nameof(IsEnabledTempRecord));
        }
        void UpdateSaved()
        {
            if (null == Saved)
                Saved = new Data();
            Saved = Current;
        }
    }
}
