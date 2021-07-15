using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    class TaskStorageUpdate : BaseSensorTask
    {
        readonly MemVarUInt16 StatReg = new MemVarUInt16(0x8802);
        public TaskStorageUpdate(SensorModel sensor)
            : base(sensor, Resource.StoragePolling)
        {
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == Connection)
                return false;

            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = Resource.Startup;

            if (!(Sensor.Storage is Storage dmgStor))
                return false;

            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
            using (var timer = CreateProgressTimer(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                await Connection.ReadAsync(StatReg, null, linkTsc.Token);
                
                dmgStor.Aviable = 0x04 == StatReg.Value || 0x05 == StatReg.Value;

                InfoEx = Resource.Successfully;
                return true;
            }
        }
    }
}
