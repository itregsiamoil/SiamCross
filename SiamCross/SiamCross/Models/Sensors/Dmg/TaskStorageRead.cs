using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    class TaskStorageRead : BaseSensorTask
    {
        readonly Storage _Storage;
        public TaskStorageRead(SensorModel sensor)
            : base(sensor, Resource.ReadingSurvey)
        {
            _Storage = sensor.Storage as Storage;
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;
            if (!await CheckConnectionAsync(ct))
                return false;
            return false;
        }
    }
}
