using System;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DynamogrammSurveyCfg : BaseSurveyCfgModel
    {
        public readonly SensorModel Sensor;
        public struct Data
        {
            public Data(UInt16 rod, UInt32 period, UInt16 apert, UInt16  travel, UInt16 pump)
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
        public Data Current = new Data(120, 4000, 1, 500, 0);

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
            set => SetProperty(ref Current.Rod, ((UInt16)(value * 10.0f)) );
        }
        public UInt32 DynPeriod
        {
            get => Current.DynPeriod / 1000;
            set => SetProperty(ref Current.DynPeriod, value * 1000);
        }
        public UInt16 ApertNumber
        {
            get => Current.ApertNumber;
            set => SetProperty(ref Current.ApertNumber, value);
        }
        public UInt16 Imtravel
        {
            get => Current.Imtravel;
            set => SetProperty(ref Current.Imtravel, value);
        }
        public UInt16 ModelPump
        {
            get => Current.ModelPump;
            set => SetProperty(ref Current.ModelPump, value);
        }

        public double PumpRate
        {
            get => Math.Round(60000.0f / Current.DynPeriod, 3);
            set => Current.DynPeriod = (UInt32)(60000.0f / value);//ChangeNotify(nameof(DynPeriod));
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
