using SiamCross.Models.Connection.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    class TaskSurveyCfgLoad : BaseSensorTask
    {
        readonly SurveyCfg _Model;

        readonly MemStruct SurvayParam = new MemStruct(0x8016);
        readonly MemVarUInt32 Interval = new MemVarUInt32();
        readonly MemVarUInt16 Revbit = new MemVarUInt16();

        public TaskSurveyCfgLoad(SurveyCfg model)
            : base(model.Sensor, "Опрос параметров измерения")
        {
            _Model = model;

            SurvayParam.Add(Interval);
            SurvayParam.Add(Revbit);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model || null == Connection)
                return false;
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
            if (_Model.Saved.HasValue)
            {
                _Model.Current = _Model.Saved.Value;
                InfoEx = "обновлено";
                return true;
            }

            bool readed = false;
            if (await CheckConnectionAsync(ct))
            {
                InfoEx = "чтение";
                readed = RespResult.NormalPkg == await Connection.
                    TryReadAsync(SurvayParam, null, ct);
                InfoEx = "выполнено";
            }
            if (readed)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Model.Current.Revbit = Revbit.Value;
                _Model.Current.Interval = Interval.Value;
            }
            else
            {
                _Model.Current.Revbit = 0;
                _Model.Current.Interval = 0;
            }
            return readed;
        }
    }
}
