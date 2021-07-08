using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskStorageClear : BaseSensorTask
    {
        readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);

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
            InfoEx = Resource.Startup;
            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
            using (var timer = CreateProgressTimer(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                OpReg.Value = 3;
                await Connection.WriteAsync(OpReg, null, linkTsc.Token);
                await Task.Delay(Constants.SecondDelay, linkTsc.Token);
                await Connection.WriteAsync(OpReg, null, linkTsc.Token);
                InfoEx = Resource.Successfully;
                return true;
            }
        }
    }
}
