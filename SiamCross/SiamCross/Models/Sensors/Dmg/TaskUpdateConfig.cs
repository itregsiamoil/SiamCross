using SiamCross.Models.Connection.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskUpdateConfig : BaseSensorTask
    {
        private readonly MemStruct _NonvolatileParam;
        private readonly MemVarFloat Nkp;
        private readonly MemVarFloat Rkp;
        private readonly MemVarFloat ZeroG;
        private readonly MemVarFloat PositiveG;
        private readonly MemVarUInt32 EnableInterval;
        private readonly MemVarFloat ZeroOffset;
        private readonly MemVarFloat SlopeRatio;
        private readonly MemVarFloat NegativeG;
        private readonly MemVarUInt16 SleepDisable;
        private readonly MemVarUInt16 SleepTimeout;
        private uint _BytesTotal;
        private uint _BytesProgress;
        private void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = (float)_BytesProgress / _BytesTotal;
        }

        public TaskUpdateConfig(SensorModel sensor)
            : base(sensor, Resource.ReadingState)
        {
            _NonvolatileParam = new MemStruct(0x8100);
            Nkp = _NonvolatileParam.Add(new MemVarFloat(nameof(Nkp)));
            Rkp = _NonvolatileParam.Add(new MemVarFloat(nameof(Rkp)));
            ZeroG = _NonvolatileParam.Add(new MemVarFloat(nameof(ZeroG)));
            PositiveG = _NonvolatileParam.Add(new MemVarFloat(nameof(PositiveG)));
            EnableInterval = _NonvolatileParam.Add(new MemVarUInt32(nameof(EnableInterval)));
            ZeroOffset = _NonvolatileParam.Add(new MemVarFloat(nameof(ZeroOffset)));
            SlopeRatio = _NonvolatileParam.Add(new MemVarFloat(nameof(SlopeRatio)));
            NegativeG = _NonvolatileParam.Add(new MemVarFloat(nameof(NegativeG)));
            SleepDisable = _NonvolatileParam.Add(new MemVarUInt16(nameof(SleepDisable)));
            SleepTimeout = _NonvolatileParam.Add(new MemVarUInt16(nameof(SleepTimeout)));
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            _BytesProgress = 0;
            _BytesTotal = _NonvolatileParam.Size;
            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }
        private async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;
            RespResult ret = RespResult.ErrorUnknown;
            try
            {
                ret = await Connection.ReadAsync(_NonvolatileParam, SetProgressBytes, ct);
                Sensor.Device.DeviceData["Nkp"] = Nkp.Value;
                Sensor.Device.DeviceData["Rkp"] = Rkp.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return RespResult.NormalPkg == ret;
        }
    }
}
