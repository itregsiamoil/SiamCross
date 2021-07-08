using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    public class TaskUpdateStatus : BaseSensorTask
    {
        readonly MemStruct _CurrentParam;
        readonly MemVarFloat Pressure;
        readonly MemVarFloat ТempInt;
        readonly MemVarFloat ТempExt;
        readonly MemVarFloat Acc;

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskUpdateStatus(SensorModel sensor)
            : base(sensor, Resource.ReadingState)
        {
            _CurrentParam = new MemStruct(0x8400);
            Pressure = _CurrentParam.Add(new MemVarFloat(nameof(Pressure)));
            ТempInt = _CurrentParam.Add(new MemVarFloat(nameof(ТempInt)));
            ТempExt = _CurrentParam.Add(new MemVarFloat(nameof(ТempExt)));
            Acc = _CurrentParam.Add(new MemVarFloat(nameof(Acc)));
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

            var battery = (Acc.Value).ToString("N2");
            var temperature = (ТempInt.Value).ToString("N2");

            var press_str = (Pressure.Value).ToString("N2");
            var exttemp_str = (ТempExt.Value).ToString("N2");

            var status =
                $"{Resource.Pressure}: " + press_str + $" ({Resource.KGFCMUnits})"
                + $"\n{Resource.ProbeTemperature}: " + exttemp_str + $" ({Resource.DegCentigradeUnits})";



            Sensor.Device.DeviceData["Battery"] = battery;
            Sensor.Device.DeviceData["Temperature"] = temperature;
            Sensor.Device.DeviceData["Status"] = Status;

            Sensor.Status.ChangeNotify("Battery");
            Sensor.Status.ChangeNotify("Temperature");
            Sensor.Status.ChangeNotify("Status");

            Info = status;

            return ret;
        }


    }
}
