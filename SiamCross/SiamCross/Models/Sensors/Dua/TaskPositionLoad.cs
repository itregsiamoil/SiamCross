using SiamCross.Models.Connection.Protocol;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskPositionLoad : BaseSensorTask
    {
        readonly SensorPosition _Model;

        readonly MemStruct SurvayParam = new MemStruct(0x800B);
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

        public TaskPositionLoad(SensorPosition pos)
            : base(pos.SensorModel, "Загрузка местоположения")
        {
            _Model = pos;

            SurvayParam.Add(Kust);
            SurvayParam.Add(Skv);
            SurvayParam.Add(Field);
            SurvayParam.Add(Shop);
            SurvayParam.Add(Operator);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model)
                return false;

            _BytesProgress = 0;
            _BytesTotal = SurvayParam.Size;

            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;

            InfoEx = "чтение";
            bool readed = RespResult.NormalPkg == await Connection.TryReadAsync(SurvayParam, SetProgressBytes, ct);

            if (readed)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Model.FieldId = Field.Value;
                _Model.Well = Encoding.UTF8.GetString(Skv.Value, 0, Skv.Value.Length);
                _Model.Bush = Encoding.UTF8.GetString(Kust.Value, 0, Kust.Value.Length);
                _Model.Shop = Shop.Value;
                InfoEx = "успешно выполнено";

            }
            else
            {
                _Model.FieldId = 0;
                _Model.Well = "0";
                _Model.Bush = "0";
                _Model.Shop = 0;
                InfoEx = "set default";
            }

            _Model.ChangeNotify(nameof(_Model.FieldId));
            _Model.ChangeNotify("SelectedField");
            _Model.ChangeNotify(nameof(_Model.Well));
            _Model.ChangeNotify(nameof(_Model.Bush));
            _Model.ChangeNotify(nameof(_Model.Shop));
            return readed;
        }

    }
}
