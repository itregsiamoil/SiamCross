using SiamCross.Models.Connection.Protocol;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class TaskStatusUpdate : BaseSensorTask
    {
        readonly MemStruct _CurrentParam= new MemStruct(0x8400);
        readonly MemVarUInt16 BatteryVoltage = new MemVarUInt16();
        readonly MemVarUInt16 ТempC = new MemVarUInt16();
        readonly MemVarInt16 Pressure = new MemVarInt16();

        public TaskStatusUpdate(SensorModel model)
           : base(model, "Обновление статуса")
        {
            _CurrentParam.Add(BatteryVoltage);
            _CurrentParam.Add(ТempC);
            _CurrentParam.Add(Pressure);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == Sensor)
                return false;


            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (await CheckConnectionAsync(ct))
                return false;

            bool readed = false;
            readed = RespResult.NormalPkg == await Connection.ReadAsync(_CurrentParam, null, ct);

            var battery = BatteryVoltage.Value / 10.0;
            var temperature = ТempC.Value / 10.0;
            var pressure = Pressure.Value / 10.0;

            var status = $"{Resource.Pressure}: "
                + Pressure.Value / 10.0 + $" ({Resource.KGFCMUnits})";


            Sensor.Device.DeviceData["Battery"] = battery;
            Sensor.Device.DeviceData["Temperature"] = temperature;
            Sensor.Device.DeviceData["Status"] = status;


            return readed;
        }
    }
}
