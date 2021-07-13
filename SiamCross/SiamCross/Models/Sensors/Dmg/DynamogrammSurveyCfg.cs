using SiamCross.Models.Tools;
using System;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DynamogrammSurveyCfg : BaseSurveyCfgModel
    {
        public readonly SensorModel Sensor;
        public struct Data
        {
            public Data(UInt16 rod, UInt32 period, UInt16 apert, UInt16 travel, UInt16 pump)
            {
                Rod = rod;
                DynPeriod = period;
                ApertNumber = apert;
                Imtravel = travel;
                ModelPump = pump;
            }
            // параметры сохраняемые в приборе
            public UInt16 Rod;//диаметр штока, 0,1мм,min 120, max 400
            public UInt32 DynPeriod;//период качания в мс, min 4000, max 180000
            public UInt16 ApertNumber;//номер отверстия, у.ед.,min 1, max 5
            public UInt16 Imtravel;  //длина хода, мм  ,min 500, max 9999
            public UInt16 ModelPump;  //тип ШГНУ, 0-балансирный привод, 1-цепной привод, 2-гидравлический привод
            // параметры сохраняемые в БД
        }
        public Data? Saved { get; private set; }

        private Data _Current = new Data(120, 4000, 0, 500, 0);
        public Data Current => _Current;

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
        public void NotyfyUpdateAll()
        {
            ChangeNotify(nameof(Rod));
            ChangeNotify(nameof(DynPeriod));
            ChangeNotify(nameof(ApertNumber));
            ChangeNotify(nameof(Imtravel));
            ChangeNotify(nameof(ModelPump));
        }

        public double Rod
        {
            get => Math.Round(Current.Rod / 10.0f, 1);
            set => CheckAndSetProperty<UInt16>(ref _Current.Rod, (UInt16)(value * 10.0f), 120, 400);
        }
        public double DynPeriod
        {
            get => Math.Round(Current.DynPeriod / 1000.0f, 3);
            set => CheckAndSetProperty<UInt32>(ref _Current.DynPeriod, (UInt32)(value * 1000), 4000, 180000);
        }
        public UInt16 ApertNumber
        {
            get => Current.ApertNumber;
            set => CheckAndSetProperty<UInt16>(ref _Current.ApertNumber, value, 0, 5);
        }
        public UInt16 Imtravel
        {
            get => Current.Imtravel;
            set => CheckAndSetProperty<UInt16>(ref _Current.Imtravel, value, 500, 9999);
        }
        public UInt16 ModelPump
        {
            get => Current.ModelPump;
            set => CheckAndSetProperty<UInt16>(ref _Current.ModelPump, value, 0, 2);
        }

        public DynamogrammSurveyCfg(SensorModel sensor)
        {
            Sensor = sensor;
            TaskLoad = new TaskSurveyCfgLoad(this);
            //TaskSave = new TaskSurveyCfgSave(this);
            //TaskWait = new TaskSurveyWait(sensor);
        }
    }
}
