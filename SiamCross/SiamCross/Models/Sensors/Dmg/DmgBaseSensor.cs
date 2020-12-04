using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public abstract class DmgBaseSensor : BaseSensor
    {
        private DmgBaseQuickReportBuiler _reportBuilder = new DmgBaseQuickReportBuiler();

        public DmgBaseSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
        public async Task<bool> UpdateFirmware(CancellationToken cancelToken)
        {
            byte[] resp;
            byte[] fw_address = new byte[4];
            byte[] fw_size = new byte[2];

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange(DmgCmd.Get("ProgrammVersionAddress")); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 4).CopyTo(fw_address);

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange(DmgCmd.Get("ProgrammVersionSize")); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 2).CopyTo(fw_size);

            cancelToken.ThrowIfCancellationRequested();
            var req = new MessageCreator().CreateReadMessage(fw_address, fw_size);
            resp = await Connection.Exchange(req); ;
            if (0 == resp.Length)
                return false;

            string dataValue;
            dataValue = GetStringPayload(resp);
            if (null == dataValue || 0 == dataValue.Length)
                return false;
            SensorData.Firmware = dataValue;
            return true;
        }
        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            //"BatteryVoltage" + "Тemperature"+"LoadChanel"+"AccelerationChanel"
            byte[] req = new byte[] { 0x0D, 0x0A, 0x01, 0x01,
                0x00, 0x84, 0x00, 0x00,    0x0C, 0x00,    0x64, 0x19 };
            byte[] resp = await Connection.Exchange(req);
            if (0 == resp.Length)
            {
                return false;
            }

            SensorData.Battery = (((float)BitConverter.ToInt16(resp, 12)) / 10).ToString();
            SensorData.Temperature = (((float)BitConverter.ToInt16(resp, 12 + 2)) / 10).ToString();
            _reportBuilder.Load = BitConverter.ToSingle(resp, 12 + 4).ToString();
            _reportBuilder.Acceleration = BitConverter.ToSingle(resp, 12 + 8).ToString();

            SensorData.Status = _reportBuilder.GetReport();
            return true;
            /*
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["AccelerationChanel"]);
            */
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
            SensorData.Status = Resource.ConnectedStatus;
            return (await UpdateFirmware(cancelToken) && await KillosParametersQuery(cancelToken));
        }

    }//DmgBaseSensor
}
