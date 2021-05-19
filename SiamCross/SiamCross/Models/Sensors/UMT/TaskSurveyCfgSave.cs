using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    public class TaskSurveyCfgSave : BaseSensorTask
    {
        readonly SurveyCfg _Model;

        readonly MemStruct SurvayParam = new MemStruct(0x8016);
        readonly MemVarUInt32 Interval = new MemVarUInt32();
        readonly MemVarUInt16 Revbit = new MemVarUInt16();
        readonly MemVarByteArray Timestamp = new MemVarByteArray(0x8426, new MemValueByteArray(6));

        public TaskSurveyCfgSave(SurveyCfg model)
            : base(model.Sensor, "Запись параметров измерения")
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
                    return await DoSaveAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> DoSaveAsync(CancellationToken ct)
        {
            if (!_Model.Saved.HasValue)
            {
                InfoEx = "параметры не считаны, обновите параметры";
                return false;
            }

            if (_Model.Saved.Equals(_Model.Current))
            {
                InfoEx = "обновлено";
                return true;
            }

            Revbit.Value = _Model.Current.Revbit;
            Interval.Value = _Model.Current.Interval;
            var dt = DateTime.Now;
            Timestamp.Value[5] = (100 > dt.Year) ? (byte)dt.Year : (byte)(dt.Year % 100);
            Timestamp.Value[4] = (byte)dt.Month;
            Timestamp.Value[3] = (byte)dt.Day;
            Timestamp.Value[0] = (byte)dt.Hour;
            Timestamp.Value[1] = (byte)dt.Minute;
            Timestamp.Value[2] = (byte)dt.Second;

            InfoEx = "запись";
            bool ret = false;
            ret = RespResult.NormalPkg == await Connection.TryWriteAsync(Timestamp, null, ct);
            if (!ret)
                return false;

            foreach (var r in SurvayParam.GetVars())
            {
                ret = RespResult.NormalPkg == await Connection.TryWriteAsync(r, null, ct);
                if (!ret)
                    return false;
            }
            InfoEx = "выполнено";
            return ret;
        }

    }
}
