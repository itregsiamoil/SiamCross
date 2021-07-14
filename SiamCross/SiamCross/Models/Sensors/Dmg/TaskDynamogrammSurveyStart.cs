using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dmg.Surveys;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskDynamogrammSurveyStart : BaseSensorTask
    {
        readonly MemVarUInt16 CtrlReg = new MemVarUInt16(0x8800);

        public TaskDynamogrammSurveyStart(SensorModel sensor)
            : base(sensor, Kind.Dynamogramm.Title())
        {
            
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = Resource.Initialization;
            CtrlReg.Value = 0x02;
            await Connection.WriteAsync(CtrlReg, null, ct);
            CtrlReg.Value = 0x01;
            await Connection.WriteAsync(CtrlReg, null, ct);
            InfoEx = Resource.Startup;
            return true;
        }
    }
}
