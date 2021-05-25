using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskUpdateStatus : BaseSensorTask
    {
        readonly MemStruct _CurrentParam = new MemStruct(0x8400);
        readonly MemVarUInt16 BatteryVoltage;
        readonly MemVarUInt16 ТempC;
        readonly MemVarInt16 Pressure;

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskUpdateStatus(SensorModel sensor)
            : base(sensor, "Чтение состояния")
        {
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            _BytesProgress = 0;
            _BytesTotal = _CurrentParam.Size;

            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }

        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;
            bool ret = RespResult.NormalPkg == await Connection.TryReadAsync(_CurrentParam, SetProgressBytes, ct);

            var Battery = (BatteryVoltage.Value / 10.0).ToString();
            var Temperature = (ТempC.Value / 10.0).ToString();
            var Status =
                  $"{Resource.Pressure}: {Pressure.Value / 10.0} ({Resource.KGFCMUnits})\n"
                + $"{Resource.Temperature}: {ТempC.Value / 10.0} ({Resource.DegCentigradeUnits})";


            Sensor.Device.DeviceData["Battery"] = Battery;
            Sensor.Device.DeviceData["Temperature"] = Temperature;
            Sensor.Device.DeviceData["Status"] = Status;

            Sensor.Status.ChangeNotify("Battery");
            Sensor.Status.ChangeNotify("Temperature");
            Sensor.Status.ChangeNotify("Status");

            Info = Status;

            return ret;
        }


    }
}
