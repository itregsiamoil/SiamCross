using SiamCross.Models.Connection.Protocol;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskPositionSave : BaseSensorTask
    {
        readonly SensorPosition _Model;

        public readonly List<MemVar> Reg = new List<MemVar>();

        readonly MemVarByteArray Kust = new MemVarByteArray(0x800B, new MemValueByteArray(5));
        readonly MemVarByteArray Skv = new MemVarByteArray(0x8010, new MemValueByteArray(6));
        readonly MemVarUInt16 Field = new MemVarUInt16(0x8016);
        readonly MemVarUInt16 Shop = new MemVarUInt16(0x8018);
        readonly MemVarUInt16 Operator = new MemVarUInt16(0x801A);

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskPositionSave(SensorPosition pos)
            : base(pos.SensorModel, "Сохранение местоположения")
        {
            _Model = pos;

            Reg.Add(Kust);
            Reg.Add(Skv);
            Reg.Add(Field);
            Reg.Add(Shop);
            Reg.Add(Operator);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model)
                return false;

            _BytesProgress = 0;
            _BytesTotal = 0;
            Reg.ForEach((r) => _BytesTotal += r.Size);

            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await DoSaveSingleAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> DoSaveSingleAsync(CancellationToken ct)
        {
            Field.Value = (ushort)_Model.FieldId;
            PadRightCharCopy(Skv.Value, _Model.Well);
            PadRightCharCopy(Kust.Value, _Model.Bush);
            Shop.Value = (ushort)_Model.Shop;
            Operator.Value = 0;
            InfoEx = "запись";
            foreach (var r in Reg)
            {
                RespResult ret
                    = await Connection.TryWriteAsync(r, SetProgressBytes, ct);
                if (RespResult.NormalPkg != ret)
                    return false;
            }
            InfoEx = "выполнено";
            return true;
        }

        void PadRightCharCopy(byte[] dst, string src)
        {
            for (int i = 0; i < dst.Length; ++i)
                dst[i] = (byte)'\0';
            for (int i = 0; i < dst.Length && i < src.Length; ++i)
                dst[i] = (byte)src[i];
        }

    }
}
