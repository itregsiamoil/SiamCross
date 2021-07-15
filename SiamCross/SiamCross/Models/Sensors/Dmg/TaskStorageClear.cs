using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskStorageClear : BaseSensorTask
    {
        readonly MemVarUInt16 CtrlReg = new MemVarUInt16(0x8800);

        public TaskStorageClear(SensorModel sensor)
            : base(sensor, Resource.ClearingMemory)
        {
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == Connection)
                return false;

            if (!await CheckConnectionAsync(ct))
                return false;
            CtrlReg.Value = 0x02;
            InfoEx = Resource.Startup;
            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
            using (var timer = CreateProgressTimer(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                await Connection.WriteAsync(CtrlReg, null, linkTsc.Token);
                InfoEx = Resource.Successfully;
                return true;
            }
        }
    }
}
