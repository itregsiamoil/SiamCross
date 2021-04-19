using SiamCross.Models.Connection.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSurvey : BaseSensorTask
    {
        readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        readonly MemVarUInt8 Vissl = new MemVarUInt8(0x800A);

        public TaskSurvey(SensorModel sensor, string name, byte type)
            : base(sensor, name)
        {
            Vissl.Value = type;
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (1 > Vissl.Value || 5 < Vissl.Value)
                return false;
            if (null == Connection)
                return false;

            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = "инициализация";
            await Connection.WriteAsync(Vissl, null, ct);
            InfoEx = "запуск";
            OpReg.Value = 1;
            await Connection.WriteAsync(OpReg, null, ct);
            return true;
        }
    }
}

