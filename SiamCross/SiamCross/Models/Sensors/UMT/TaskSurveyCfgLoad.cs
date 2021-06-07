using SiamCross.Models.Connection.Protocol;
using System;
using System.Diagnostics;
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

        readonly MemVarByteArray Timestamp = new MemVarByteArray(0x8426, new MemValueByteArray(6));
        readonly MemVarFloat ExTemp = new MemVarFloat(0x8408);

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }
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

            _BytesProgress = 0;
            _BytesTotal = SurvayParam.Size + Timestamp.Size + ExTemp.Size;

            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    bool ret = await UpdateAsync(linkTsc.Token);
                    _Model.ChangeNotify(nameof(_Model.Period));
                    _Model.ChangeNotify(nameof(_Model.IsEnabledTempRecord));
                    _Model.ChangeNotify(nameof(_Model.IsEnabledExtTemp));
                    return ret;
                }
            }
        }
        async Task LoadTimestamp(CancellationToken ct)
        {
            RespResult res = RespResult.ErrorUnknown;
            InfoEx = "чтение времени";
            res = await Connection.TryReadAsync(Timestamp, SetProgressBytes, ct);

            if (RespResult.NormalPkg == res)
            {
                var dt = DateTime.Now;
                var epoh = dt.Year - dt.Year % 100;
                var year = (100 > Timestamp.Value[5]) ? epoh + Timestamp.Value[5] : Timestamp.Value[5];

                try
                {
                    _Model.Timestamp = new DateTime(
                        year, Timestamp.Value[4], Timestamp.Value[3]
                        , Timestamp.Value[0], Timestamp.Value[1], Timestamp.Value[2]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: invalid date {ex.Message}");
                    _Model.Timestamp = DateTime.MinValue;
                }
            }
            else
                _Model.Timestamp = DateTime.MinValue;
            _Model.ChangeNotify(nameof(_Model.Timestamp));
        }
        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;
            await LoadTimestamp(ct);

            if (_Model.Saved.HasValue)
            {
                _Model.Current = _Model.Saved.Value;
                InfoEx = "обновлено";
                return true;
            }

            bool readed = false;
            InfoEx = "чтение параметров";
            if (RespResult.NormalPkg == await Connection.TryReadAsync(SurvayParam, SetProgressBytes, ct)
                && RespResult.NormalPkg == await Connection.TryReadAsync(ExTemp, SetProgressBytes, ct))
                readed = true;

            if (!readed)
            {
                _Model.Current.Revbit = 0;
                _Model.Current.Interval = 0;
                _Model.Current.IsExtetnalTemp = false;
                _Model.ResetSaved();
                return false;
            }

            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _Model.Current.Revbit = Revbit.Value;
            _Model.Current.Interval = Interval.Value;
            _Model.Current.IsExtetnalTemp = !(-300 == ExTemp.Value);
            _Model.UpdateSaved();
            InfoEx = "выполнено";
            return readed;
        }
    }
}
