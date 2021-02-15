using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Connection.Protocol.Siam;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public abstract class DmgBaseSensor : BaseSensor
    {
        private readonly DmgBaseQuickReportBuiler _reportBuilder = new DmgBaseQuickReportBuiler();


        readonly IProtocolConnection ProtConn;
        readonly byte[] _membuf = new byte[1024];
        
        static void ThrowOnError(RespResult ret)
        {
            switch (ret)
            {
                case RespResult.NormalPkg: break;
                case RespResult.ErrorCrc: throw new IOCrcException();
                case RespResult.ErrorPkg: throw new IOErrPkgException();
                case RespResult.ErrorTimeout: throw new IOTimeoutException();
                case RespResult.ErrorSending: 
                case RespResult.ErrorConnection:
                case RespResult.ErrorUnknown:
                default: throw new ProtocolException();
            }
        }
        async Task ReadMem(MemItem item)
        {
            RespResult ret = await ProtConn.TryReadMemoryAsync(item.Address, item.Size, _membuf);
            ThrowOnError(ret);
            item.FromArray(_membuf);
        }
        async Task WriteMem(MemItem item)
        {
            RespResult ret =  await ProtConn.TryWriteMemoryAsync(item.Address, item.Size, item.ToArray());
            ThrowOnError(ret);
        }
        
        readonly MemStruct _CommonReg;
        readonly MemVarUInt16 DeviceType;
        readonly MemVarUInt16 MemoryModelVersion;
        readonly MemVarUInt32 DeviceNameAddress;
        readonly MemVarUInt16 DeviceNameSize;
        readonly MemVarUInt32 DeviceNumber;

        readonly MemStruct _InfoReg;
        readonly MemVarUInt32 ProgrammVersionAddress;
        readonly MemVarUInt16 ProgrammVersionSize;

        readonly MemStruct _SurvayReg;




        readonly MemStruct _CurrentParamReg;
        readonly MemVarUInt16 BatteryVoltage;
        readonly MemVarInt16 Тemperature;
        readonly MemVarFloat LoadChanel;
        readonly MemVarFloat AccelerationChanel;


        public DmgBaseSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
            ProtConn = new SiamConnection(Connection.PhyConnection, 1);

            _CommonReg = new MemStruct(0);
            DeviceType = _CommonReg.Add(new MemVarUInt16(), nameof(DeviceType));
            MemoryModelVersion = _CommonReg.Add(new MemVarUInt16(), nameof(MemoryModelVersion));
            DeviceNameAddress = _CommonReg.Add(new MemVarUInt32(), nameof(DeviceNameAddress));
            DeviceNameSize = _CommonReg.Add(new MemVarUInt16(), nameof(DeviceNameSize));
            DeviceNumber = _CommonReg.Add(new MemVarUInt32(), nameof(DeviceNumber));

            _InfoReg = new MemStruct(0x1000);
            ProgrammVersionAddress = _InfoReg.Add(new MemVarUInt32(), nameof(ProgrammVersionAddress));
            ProgrammVersionSize = _InfoReg.Add(new MemVarUInt16(), nameof(ProgrammVersionSize));

            _CurrentParamReg = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParamReg.Add(new MemVarUInt16(), nameof(BatteryVoltage));
            Тemperature = _CurrentParamReg.Add(new MemVarInt16(), nameof(Тemperature));
            LoadChanel = _CurrentParamReg.Add(new MemVarFloat(), nameof(LoadChanel));
            AccelerationChanel = _CurrentParamReg.Add(new MemVarFloat(), nameof(AccelerationChanel));


        }

        public async Task<bool> UpdateFirmware(CancellationToken cancelToken)
        {
            try 
            {
                await ReadMem(_InfoReg);
                UInt32 fw_address = ProgrammVersionAddress.Value;
                UInt16 fw_size = ProgrammVersionSize.Value;

                var ret = await ProtConn.TryReadMemoryAsync(fw_address, fw_size, _membuf);
                ThrowOnError(ret);
                SensorData.Firmware = Encoding.UTF8.GetString(_membuf, 0, fw_size);
                return true;
            }
            catch(ProtocolException ex)
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
        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                await ReadMem(_CurrentParamReg);

                SensorData.Battery = (BatteryVoltage.Value / 10.0).ToString();
                SensorData.Temperature = (Тemperature.Value / 10.0).ToString();
                _reportBuilder.Load = LoadChanel.Value.ToString();
                _reportBuilder.Acceleration = AccelerationChanel.Value.ToString();
                SensorData.Status = _reportBuilder.GetReport();
                return true;
            }
            catch (ProtocolException ex)
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
        public async Task<bool> KillosParametersQuery(CancellationToken cancelToken)
        {
            byte[] resp;
            //Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();
            //string cmd;
            string dataValue;

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange(DmgCmd.Get("SensorLoadRKP")); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = BitConverter.ToSingle(resp, 12).ToString();
            _reportBuilder.SensitivityLoad = dataValue;

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange(DmgCmd.Get("SensorLoadNKP")); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = BitConverter.ToSingle(resp, 12).ToString();
            _reportBuilder.ZeroOffsetLoad = dataValue;
            return true;

            //await mConnection.SendData(Ddin2Commands.FullCommandDictionary["SensorLoadRKP"]);
            //await mConnection.SendData(Ddin2Commands.FullCommandDictionary["SensorLoadNKP"]);
        }
        public override async Task<bool> PostConnectInit(CancellationToken cancelToken)
        {
            await ProtConn.Connect();
            SensorData.Status = Resource.ConnectedStatus;
            return (await UpdateFirmware(cancelToken) && await KillosParametersQuery(cancelToken));
        }

    }//DmgBaseSensor
}
