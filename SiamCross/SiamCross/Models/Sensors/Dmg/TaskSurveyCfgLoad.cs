using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Tools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskSurveyCfgLoad : BaseSensorTask
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
        public TaskSurveyCfgLoad(DynamogrammSurveyCfg model)
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
                    bool ret = await UpdateAsync(linkTsc.Token);
                    //_Model.NotyfyUpdateAll();
                    return ret;
                }
            }
        }

        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;

            /*
            if (_Model.Saved.HasValue)
            {
                _Model.Current = _Model.Saved.Value;
                InfoEx = Resource.Updated;
                return true;
            }
            */

            bool readed = false;
            InfoEx = Resource.ReadingParameters;
            if (RespResult.NormalPkg == await Connection.TryReadAsync(SurvayParam, SetProgressBytes, ct))
                readed = true;

            if (!readed)
            {
                _Model.ResetSaved();
                return false;
            }

            _Model.Rod = Rod.Value;
            _Model.DynPeriod = DynPeriod.Value;
            _Model.ApertNumber = ApertNumber.Value;
            _Model.Imtravel = Imtravel.Value;
            _Model.ModelPump = ModelPump.Value;
            _Model.UpdateSaved();
            InfoEx = Resource.Complete;
            return readed;
        }


    }
}
