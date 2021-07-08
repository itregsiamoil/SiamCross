using SiamCross.Models.Connection.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskPositionSave : BaseSensorTask
    {
        readonly SensorPosition _Model;

        readonly MemStruct PosParam;
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

        public TaskPositionSave(SensorPosition pos, uint baseAddress = 0x800B)
            : base(pos.Sensor, Resource.SavingLocation)
        {
            _Model = pos;

            PosParam = new MemStruct(baseAddress);
            PosParam.Add(Kust);
            PosParam.Add(Skv);
            PosParam.Add(Field);
            PosParam.Add(Shop);
            PosParam.Add(Operator);

        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model)
                return false;

            _BytesProgress = 0;
            _BytesTotal = PosParam.Size;

            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    bool ret = await DoSaveSingleAsync(linkTsc.Token);
                    if (ret)
                        _Model.UpdateSaved();
                    else
                        _Model.ResetSaved();
                    return ret;
                }
            }
        }
        async Task<bool> DoSaveSingleAsync(CancellationToken ct)
        {
            Field.Value = (ushort)_Model.Current.Field;
            PadRightCharCopy(Skv.Value, _Model.Current.Well);
            PadRightCharCopy(Kust.Value, _Model.Current.Bush);
            Shop.Value = (ushort)_Model.Current.Shop;
            Operator.Value = 0;
            InfoEx = Resource.Recording;
            bool ret = false;
            foreach (var r in PosParam.GetVars())
            {
                ret = RespResult.NormalPkg == await Connection.TryWriteAsync(r, SetProgressBytes, ct);
                if (!ret)
                    break;
            }
            if (ret)
                InfoEx = Resource.Complete;
            return ret;
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
