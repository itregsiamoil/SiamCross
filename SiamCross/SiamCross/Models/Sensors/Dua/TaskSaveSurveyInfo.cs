using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dua.Surveys;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSaveSurveyInfo : BaseSensorTask
    {
        readonly Level _Model;

        public readonly MemStruct SurvayParam = new MemStruct(0x8008);
        public readonly MemVarUInt16 Revbit = new MemVarUInt16();
        public readonly MemVarUInt8 Vissl = new MemVarUInt8();
        public readonly MemVarByteArray Kust = new MemVarByteArray(null, 0, new MemValueByteArray(5));
        public readonly MemVarByteArray Skv = new MemVarByteArray(null, 0, new MemValueByteArray(6));
        public readonly MemVarUInt16 Field = new MemVarUInt16();
        public readonly MemVarUInt16 Shop = new MemVarUInt16();
        public readonly MemVarUInt16 Operator = new MemVarUInt16();
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16();
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16();
        public readonly MemVarUInt8 PerP = new MemVarUInt8();
        public readonly MemVarUInt8 KolP = new MemVarUInt8();
        public readonly MemVarByteArray PerU = new MemVarByteArray(null, 0, new MemValueByteArray(5));
        public readonly MemVarByteArray KolUr = new MemVarByteArray(null, 0, new MemValueByteArray(5));

        public TaskSaveSurveyInfo(Level model, ISensor sensor)
            : base(sensor, "Запись параметров измерения")
        {
            _Model = model;
            SurvayParam.Add(Revbit);
            SurvayParam.Add(Vissl);
            SurvayParam.Add(Kust);
            SurvayParam.Add(Skv);
            SurvayParam.Add(Field);
            SurvayParam.Add(Shop);
            SurvayParam.Add(Operator);
            SurvayParam.Add(Vzvuk);
            SurvayParam.Add(Ntpop);
            SurvayParam.Add(PerP);
            SurvayParam.Add(KolP);
            SurvayParam.Add(PerU);
            SurvayParam.Add(KolUr);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Model || null == Connection || null == Sensor)
                return false;
            using (var timer = CreateProgressTimer(25000))
                return await DoSave();
        }
        public async Task<bool> DoSave()
        {
            bool ret = false;
            if (await CheckConnectionAsync())
            {
                InfoEx = "запись";

                BitVector32 myBV = new BitVector32(Revbit.Value);
                int bit0 = BitVector32.CreateMask();
                int bit1 = BitVector32.CreateMask(bit0);
                int bit2 = BitVector32.CreateMask(bit1);
                int bit3 = BitVector32.CreateMask(bit2);
                int bit4 = BitVector32.CreateMask(bit3);
                int bit5 = BitVector32.CreateMask(bit4);
                int bit6 = BitVector32.CreateMask(bit5);
                int bit7 = BitVector32.CreateMask(bit6);
                int bit8 = BitVector32.CreateMask(bit7);
                int bit9 = BitVector32.CreateMask(bit8);

                myBV[bit5] = _Model.IsAutoswitchToAPR;
                myBV[bit1] = _Model.IsValveAutomaticEnabled;
                myBV[bit2] = _Model.IsValveDurationShort;
                myBV[bit0] = _Model.IsValveDirectionInput;
                myBV[bit6] = _Model.IsPiezoDepthMax;
                myBV[bit9] = _Model.IsPiezoAdditionalGain;

                Revbit.Value = (UInt16)myBV.Data;
                Vissl.Value = _Model.SurveyType;
                Vzvuk.Value = (UInt16)(_Model.SoundSpeedFixed * 10);
                Ntpop.Value = _Model.SoundSpeedTableId;

                PerP.Value = _Model.PressurePeriodIndex;
                KolP.Value = _Model.PressureDelayIndex;

                _Model.LevelPeriodIndex.CopyTo(PerU.Value, 0);
                _Model.LevelDelayIndex.CopyTo(KolUr.Value, 0);

                ret = await RetryExecAsync(3, DoSaveSingle);
            }

            _Model.Synched = ret;
            if (ret)
            {
                InfoEx = "успешно выполнено";
                
            }
            return ret;
        }
        async Task<bool> DoSaveSingle()
        {
            foreach(var p in SurvayParam.GetVars())
            {
                RespResult ret
                    = await Connection.TryWriteAsync(p, null, _Cts.Token);
                if (RespResult.NormalPkg != ret)
                    return false;
            }
            return true;
        }
    }
}
