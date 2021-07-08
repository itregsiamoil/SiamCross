using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskUpdateStatus : BaseSensorTask
    {
        readonly MemStruct _CurrentParam;
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarInt16 Тemperature;
        public readonly MemVarFloat LoadChanel;
        public readonly MemVarFloat AccelerationChanel;

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = (float)_BytesProgress / _BytesTotal;
        }

        public TaskUpdateStatus(SensorModel sensor)
            : base(sensor, "Чтение состояния")
        {
            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            Тemperature = _CurrentParam.Add(new MemVarInt16(nameof(Тemperature)));
            LoadChanel = _CurrentParam.Add(new MemVarFloat(nameof(LoadChanel)));
            AccelerationChanel = _CurrentParam.Add(new MemVarFloat(nameof(AccelerationChanel)));
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

            await Sensor.Connection.PhyConnection.UpdateRssi();

            bool ret = RespResult.NormalPkg == await Connection.TryReadAsync(_CurrentParam, SetProgressBytes, ct);

            var battery = (BatteryVoltage.Value / 10.0).ToString("N2");
            var temperature = (Тemperature.Value / 10.0).ToString("N2");

            var status = GetLoadSting() + "\n" + GetAccelerationSting();

            Sensor.Device.DeviceData["Battery"] = battery;
            Sensor.Device.DeviceData["Temperature"] = temperature;
            Sensor.Device.DeviceData["Status"] = Status;
            Sensor.Status.ChangeNotify("Battery");
            Sensor.Status.ChangeNotify("Temperature");
            Sensor.Status.ChangeNotify("Status");
            Info = status;
            return ret;
        }
        public string GetLoadSting()
        {
            if (!Sensor.Device.TryGetData("Nkp", out float nkp)
                || !Sensor.Device.TryGetData("Rkp", out float rkp))
                return string.Empty;

            float load_mv = LoadChanel.Value;
            string ret = $"{Resource.Load}: {Math.Round(load_mv, 2)}, {Resource.MilliVoltsUnits}";
            if (rkp != 0)
            {
                float load_kg = (LoadChanel.Value - nkp) / rkp;
                ret += $" \t {Math.Round(load_kg, 2)}, {Resource.Kilograms}";
            }
            return ret;
        }
        public string GetAccelerationSting()
        {
            float _acceleration = AccelerationChanel.Value;
            string ret = $"{Resource.Acceleration}: {Math.Round(_acceleration, 2)}, {Resource.MilliVoltsUnits}";
            return ret;
        }



    }
}
