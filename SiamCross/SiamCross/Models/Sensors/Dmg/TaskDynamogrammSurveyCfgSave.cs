using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskDynamogrammSurveyCfgSave: BaseSensorTask
    {
        private readonly DynamogrammSurveyCfg _Model;

        private readonly MemStruct SurvayParam = new MemStruct(0x8000);
        private readonly MemVarUInt16 Rod = new MemVarUInt16();
        private readonly MemVarUInt32 DynPeriod = new MemVarUInt32();
        private readonly MemVarUInt16 ApertNumber = new MemVarUInt16();
        private readonly MemVarUInt16 Imtravel = new MemVarUInt16();
        private readonly MemVarUInt16 ModelPump = new MemVarUInt16();

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }
        public TaskDynamogrammSurveyCfgSave(DynamogrammSurveyCfg model)
            : base(model.Sensor, Resource.Survey_parameters)
        {
            _Model = model;

            Rod = SurvayParam.Add(Rod);
            DynPeriod = SurvayParam.Add(DynPeriod);
            ApertNumber = SurvayParam.Add(ApertNumber);
            Imtravel = SurvayParam.Add(Imtravel);
            ModelPump = SurvayParam.Add(ModelPump);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model || null == Connection)
                return false;

            _BytesProgress = 0;
            _BytesTotal = SurvayParam.Size;

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
            if (!await CheckConnectionAsync(ct))
                return false;

            if (!_Model.Saved.HasValue)
            {
                InfoEx = Resource.ParametersNotReading;
                return false;
            }

            InfoEx = Resource.RecordingParameters;
            /*
            Если не записывать заново данные измерения в динамограф -оне не запустится
            if (_Model.Saved.Equals(_Model.Current))
            {
                InfoEx = Resource.Updated;
                return true;
            }
            */
            bool ret = false;
            _Model.ResetSaved();

            Rod.Value = (UInt16)(_Model.Rod * 10.0f);
            DynPeriod.Value = (UInt32)(_Model.DynPeriod * 1000);
            ApertNumber.Value = _Model.ApertNumber;
            Imtravel.Value = _Model.Imtravel;
            ModelPump.Value = _Model.ModelPump;

            UInt16 ver = 0;
            if(Sensor.Device.TryGetData<UInt16>("MemoryModelVersion", out ver) && 10 < ver)
            {
                ret = RespResult.NormalPkg == await Connection.TryWriteAsync(SurvayParam, SetProgressBytes, ct);
                if (!ret)
                    return false;
            }
            else
            {
                foreach (var r in SurvayParam.GetVars())
                {
                    ret = RespResult.NormalPkg == await Connection.TryWriteAsync(r, SetProgressBytes, ct);
                    if (!ret)
                        return false;
                }
            }
            _Model.UpdateSaved();
            InfoEx = Resource.Complete;
            return ret;
        }

    }
}
