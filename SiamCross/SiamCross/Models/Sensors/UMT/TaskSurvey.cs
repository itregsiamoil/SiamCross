using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Umt.Surveys;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    public class TaskSurvey : BaseSensorTask
    {
        readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        readonly MemVarUInt8 Vissl = new MemVarUInt8(0x8000);

        public TaskSurvey(SensorModel sensor, string name, Kind kind)
            : base(sensor, name)
        {
            Vissl.Value = kind.ToByte();
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (1 > Vissl.Value || 4 < Vissl.Value)
                return false;
            if (null == Connection)
                return false;

            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = Resource.Initialization;
            await Connection.WriteAsync(Vissl, null, ct);
            InfoEx = Resource.Startup;
            OpReg.Value = 1;
            await Connection.WriteAsync(OpReg, null, ct);
            return true;
        }
    }
}
