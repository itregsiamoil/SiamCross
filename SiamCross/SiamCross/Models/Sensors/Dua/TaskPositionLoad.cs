using SiamCross.Models.Connection.Protocol;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskPositionLoad : BaseSensorTask
    {
        readonly SensorPosition _Model;

        readonly MemStruct PosParam;
        readonly MemVarByteArray Kust = new MemVarByteArray(0, new MemValueByteArray(5));
        readonly MemVarByteArray Skv = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt16 Field = new MemVarUInt16();
        readonly MemVarUInt16 Shop = new MemVarUInt16();
        readonly MemVarUInt16 Operator = new MemVarUInt16();

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskPositionLoad(SensorPosition pos, uint baseAddress = 0x800B)
            : base(pos.Sensor, "Загрузка местоположения")
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
                    bool ret = await UpdateAsync(linkTsc.Token);
                    if (ret)
                        _Model.UpdateSaved();
                    return ret;
                }
            }
        }
        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (null != _Model.Saved)
            {
                _Model.Current = _Model.Saved;
                InfoEx = "обновлено";
                return true;
            }

            bool readed = false;
            if (await CheckConnectionAsync(ct))
            {
                InfoEx = "чтение";
                readed = RespResult.NormalPkg == await Connection.TryReadAsync(PosParam, SetProgressBytes, ct);
                InfoEx = "выполнено";
            }

            if (readed)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Model.Current = new Position(
                    Field.Value,
                    Encoding.UTF8.GetString(Skv.Value, 0, Skv.Value.Length),
                    Encoding.UTF8.GetString(Kust.Value, 0, Kust.Value.Length),
                    Shop.Value);
            }
            else
            {
                _Model.Current = new Position();
            }

            return readed;
        }

    }
}
