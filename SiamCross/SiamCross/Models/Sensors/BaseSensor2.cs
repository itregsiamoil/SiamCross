using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSensor2 : BaseSensor
    {
        public readonly MemStruct _Common;
        public readonly MemVarUInt16 DeviceType;
        public readonly MemVarUInt16 MemoryModelVersion;
        public readonly MemVarUInt32 DeviceNameAddress;
        public readonly MemVarUInt16 DeviceNameSize;
        public readonly MemVarUInt32 DeviceNumber;
        public readonly MemStruct _Info;
        public readonly MemVarUInt32 ProgrammVersionAddress;
        public readonly MemVarUInt16 ProgrammVersionSize;



        public BaseSensor2(IProtocolConnection conn, ScannedDeviceInfo dev_info)
            : base(conn, dev_info)
        {
            _Common = new MemStruct(0x00);
            DeviceType = _Common.Add(new MemVarUInt16(nameof(DeviceType)));
            MemoryModelVersion = _Common.Add(new MemVarUInt16(nameof(MemoryModelVersion)));
            DeviceNameAddress = _Common.Add(new MemVarUInt32(nameof(DeviceNameAddress)));
            DeviceNameSize = _Common.Add(new MemVarUInt16(nameof(DeviceNameSize)));
            DeviceNumber = _Common.Add(new MemVarUInt32(nameof(DeviceNumber)));

            _Info = new MemStruct(0x1000);
            ProgrammVersionAddress = _Info.Add(new MemVarUInt32(nameof(ProgrammVersionAddress)));
            ProgrammVersionSize = _Info.Add(new MemVarUInt16(nameof(ProgrammVersionSize)));

            _Memory.Add(_Common);

            if (ScannedDeviceInfo.Device.PhyData.TryGetValue("ModemVersion", out object _))
                Connection.MaxReqLen = 230;// 247 - 3(bt header) - 12(siam header)-2(crc)
            else
                Connection.MaxReqLen = 40;
        }

        public async Task<bool> UpdateFirmware(CancellationToken cancelToken)
        {
            RespResult ret;
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                //DeviceNumber.Value = 170;
                //MemStruct ms = new MemStruct(0x0A);
                //ms.Add(DeviceNumber);
                //ret = await ProtConn.WriteAsync(ms);

                ret = await Connection.ReadAsync(_Common);
                ret = await Connection.ReadAsync(_Info);
                UInt32 fw_address = ProgrammVersionAddress.Value;
                UInt16 fw_size = ProgrammVersionSize.Value;
                byte[] membuf = new byte[fw_size];
                ret = await Connection.ReadMemAsync(fw_address, fw_size, membuf);
                Firmware = Encoding.UTF8.GetString(membuf, 0, fw_size);

                ChangeNotify(nameof(Firmware));
                ScannedDeviceInfo.Device.DeviceData["Firmware"] = Firmware;
                return true;
            }
            catch (ProtocolException)
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return false;
        }

        public override async Task<bool> PostConnectInit(CancellationToken cancelToken)
        {
            await Connection.Connect();
            Status = Resource.ConnectedStatus;
            return (await UpdateFirmware(cancelToken));
        }

        public override Task StartMeasurement(object measurementParameters)
        {
            throw new NotImplementedException();
        }
    }//DmgBaseSensor
}
