using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Sensor : BaseSensor
    {
        private Ddin2MeasurementManager _measurementManager;

        private Ddin2QuickReportBuiler _reportBuilder = new Ddin2QuickReportBuiler();

        //private Ddin2Parser _parser = new Ddin2Parser();


        public Ddin2Sensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }

        public async Task<bool> UpdateFirmware()
        {
            byte[] resp;
            byte[] fw_address = new byte[4];
            byte[] fw_size = new byte[2];

            resp = await Connection.Exchange(DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"]); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 4).CopyTo(fw_address);

            resp = await Connection.Exchange(DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 2).CopyTo(fw_size);

            var req = new MessageCreator().CreateReadMessage(fw_address, fw_size);
            resp = await Connection.Exchange(req); ;
            if (0 == resp.Length)
                return false;

            string dataValue;
            dataValue = Ddin2Parser.ConvertToStringPayload(resp);
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

            resp = await Connection.Exchange(Ddin2Commands.FullCommandDictionary["SensorLoadRKP"]); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = Ddin2Parser.ConvertToStringPayload(resp);
            _reportBuilder.SensitivityLoad = dataValue;

            resp = await Connection.Exchange(Ddin2Commands.FullCommandDictionary["SensorLoadNKP"]); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = Ddin2Parser.ConvertToStringPayload(resp);
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
            Ddin2MeasurementStartParameters specificMeasurementParameters =
                (Ddin2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddin2MeasurementManager(this, specificMeasurementParameters);
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
