using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Sensors.Dmg.SiddosA3M.Measurement;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SiamCross.Models.Sensors;
using SiamCross.Models.Tools;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M
{
    public class SiddosA3MSensor : BaseSensor
    {
        private SiddosA3MMeasurementManager _measurementManager;
        private DmgBaseQuickReportBuiler _reportBuilder = new DmgBaseQuickReportBuiler();

        public SiddosA3MSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
        public string GetStringPayload(byte[] pkg)
        {
            Span<byte> payload = pkg.AsSpan(12, pkg.Length - 12 - 2);
            if (payload.Length > 20)
                return Encoding.UTF8.GetString(payload.ToArray());
            return Encoding.GetEncoding(1251).GetString(payload.ToArray());
        }
        public async Task<bool> UpdateFirmware()
        {
            byte[] resp;
            byte[] fw_address = new byte[4];
            byte[] fw_size = new byte[2];

            resp = await Connection.Exchange(DmgCmd.Get("ProgrammVersionAddress")); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 4).CopyTo(fw_address);

            resp = await Connection.Exchange(DmgCmd.Get("ProgrammVersionSize")); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 2).CopyTo(fw_size);

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
        public override async Task<bool> QuickReport()
        {
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
        public async Task<bool> KillosParametersQuery()
        {
            byte[] resp;
            //Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();
            //string cmd;
            string dataValue;

            resp = await Connection.Exchange(DmgCmd.Get("SensorLoadRKP")); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = BitConverter.ToSingle(resp, 12).ToString();
            _reportBuilder.SensitivityLoad = dataValue;

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
        public override async Task<bool> PostConnectInit()
        {
            System.Diagnostics.Debug.WriteLine("Ддин2 успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
            return (await UpdateFirmware() && await KillosParametersQuery());
        }
        public override async Task StartMeasurement(object measurementParameters)
        {
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            SiddosA3MMeasurementStartParameters specificMeasurementParameters =
                (SiddosA3MMeasurementStartParameters)measurementParameters;
            _measurementManager = new SiddosA3MMeasurementManager(this, specificMeasurementParameters);
            var report = await _measurementManager.RunMeasurement();
            if (null != report)
            {
                SensorService.Instance.MeasurementHandler(report);
                SensorData.Status = "measure [100%] - end";
            }
            else
            {
                SensorData.Status = "measure [---%] - ERROR";
            }
            await Task.Delay(2000);
            IsMeasurement = false;
        }

    }
}