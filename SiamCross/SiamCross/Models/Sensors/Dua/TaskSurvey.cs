using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSurvey : BaseSensorTask
    {
        public readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        public readonly MemVarUInt8 Vissl = new MemVarUInt8(0x800A);

        public TaskSurvey(ISensor sensor, string name, byte type)
            : base(sensor, name)
        {
            Vissl.Value = type;
        }

        public override async Task<bool> DoExecute()
        {
            if (1 > Vissl.Value || 5 < Vissl.Value)
                return false;
            if (null == Connection || null == Sensor)
                return false;

            if (!await CheckConnectionAsync())
                return false;
            InfoEx = "инициализация";
            await Connection.WriteAsync(Vissl, null, _Cts.Token);
            InfoEx = "запуск";
            OpReg.Value = 1;
            await Connection.WriteAsync(OpReg, null, _Cts.Token);
            return true;
        }
        
    }
}

