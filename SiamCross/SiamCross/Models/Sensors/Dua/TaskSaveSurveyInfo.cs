using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSaveSurveyInfo : BaseSensorTask
    {
        readonly DuaSurveyCfg _Model;

        public readonly List<MemVar> Reg = new List<MemVar>();

        public readonly MemVarUInt16 Revbit = new MemVarUInt16(0x8008);
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16(0x801C);
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16(0x801E);
        public readonly MemVarUInt8 PerP = new MemVarUInt8(0x8020);
        public readonly MemVarUInt8 KolP = new MemVarUInt8(0x8021);
        public readonly MemVarByteArray PerU = new MemVarByteArray(0x8022, new MemValueByteArray(5));
        public readonly MemVarByteArray KolUr = new MemVarByteArray(0x8027, new MemValueByteArray(5));

        uint _BytesTotal;
        uint _BytesProgress;

        public TaskSaveSurveyInfo(DuaSurveyCfg model, SensorModel sensor)
            : base(sensor, "Запись параметров измерения")
        {
            _Model = model;
            Reg.Add(Revbit);
            Reg.Add(Vzvuk);
            Reg.Add(Ntpop);
            Reg.Add(PerP);
            Reg.Add(KolP);
            Reg.Add(PerU);
            Reg.Add(KolUr);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model || null == Connection)
                return false;

            _BytesTotal = 0;
            _BytesProgress = 0;
            Reg.ForEach((r) => _BytesTotal += r.Size);
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await DoSaveAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> DoSaveAsync(CancellationToken ct)
        {
            bool ret = false;
            if (await CheckConnectionAsync(ct))
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

                Vzvuk.Value = (UInt16)(_Model.SoundSpeedFixed * 10);
                Ntpop.Value = _Model.SoundSpeedTableId;

                PerP.Value = _Model.PressurePeriodIndex;
                KolP.Value = _Model.PressureQuantityIndex;

                _Model.LevelPeriodIndex.CopyTo(PerU.Value, 0);
                _Model.LevelQuantityIndex.CopyTo(KolUr.Value, 0);

                ret = await RetryExecAsync(3, DoSaveSingleAsync, ct);
            }

            _Model.Synched = ret;
            if (ret)
            {
                InfoEx = "успешно выполнено";

            }
            return ret;
        }
        async Task<bool> DoSaveSingleAsync(CancellationToken ct)
        {
            foreach (var r in Reg)
            {
                RespResult ret
                    = await Connection.TryWriteAsync(r, SetProgressBytes, ct);
                if (RespResult.NormalPkg != ret)
                    return false;
            }
            return true;
        }

        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }
    }
}
