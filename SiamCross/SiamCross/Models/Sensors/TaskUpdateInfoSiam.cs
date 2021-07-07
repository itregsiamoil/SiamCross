using SiamCross.Models.Connection.Protocol;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public class TaskUpdateInfoSiam : BaseSensorTask
    {
        public readonly MemStruct _Info;
        public readonly MemVarUInt32 ProgrammVersionAddress;
        public readonly MemVarUInt16 ProgrammVersionSize;

        public TaskUpdateInfoSiam(SensorModel sensor)
            : base(sensor, "Чтение состояния")
        {
            _Info = new MemStruct(0x1000);
            ProgrammVersionAddress = _Info.Add(new MemVarUInt32(nameof(ProgrammVersionAddress)));
            ProgrammVersionSize = _Info.Add(new MemVarUInt16(nameof(ProgrammVersionSize)));
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
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
            RespResult ret;
            ret = await Connection.ReadAsync(_Info);
            UInt32 fw_address = ProgrammVersionAddress.Value;
            UInt16 fw_size = ProgrammVersionSize.Value;
            byte[] membuf = new byte[fw_size];
            ret = await Connection.ReadMemAsync(fw_address, fw_size, membuf);
            var firmware = Encoding.UTF8.GetString(membuf, 0, fw_size);

            Sensor.Device.DeviceData["Firmware"] = firmware;
            Sensor.Status.ChangeNotify("Firmware");

            return RespResult.NormalPkg == ret;
        }
    }
}
