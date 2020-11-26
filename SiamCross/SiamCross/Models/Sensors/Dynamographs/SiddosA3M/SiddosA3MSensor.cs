using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
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

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M
{
    public class SiddosA3MSensor : BaseSensor
    {
        SiddosA3MMeasurementManager _measurementManager;

        private SiddosA3MQuickReportBuilder _reportBuilder;

        private SiddosA3MParser _parser = new SiddosA3MParser();
        //private FirmWaveQualifier _firmwareQualifier;


        public SiddosA3MSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {

            //IsMeasurement = false;
            //IsAlive = false;
            //SensorData = sensorData;
            //_firmwareQualifier = new FirmWaveQualifier(
            //    BluetoothAdapter.SendData,
            //    DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
            //    DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            //);
            //_parser = new SiddosA3MParser(_firmwareQualifier, true);
            _reportBuilder = new SiddosA3MQuickReportBuilder();



            Connection.DataReceived += _parser.ByteProcess;
            //_parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;
            //_parser.ExportMemoryFragment += MemoryRecieveHandler;

            //BluetoothAdapter.ConnectSucceed += ConnectHandler;
            //BluetoothAdapter.ConnectFailed += ConnectFailedHandler;


        }



        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            //_firmwareQualifier = new FirmWaveQualifier(
            //    BluetoothAdapter.SendData,
            //    DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
            //    DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            //);
            //_parser = new SiddosA3MParser(_firmwareQualifier, true);
            //_parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("СиддосА3М успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(string commandName, string dataValue)
        {
            if (!Activate)
            {
                ClearStatus();
                return;
            }

            switch (commandName) // TODO: replace to enum 
            {
                case "DeviceStatus":
                    /*/ Для замера /*/
                    if (_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = DynamographStatusAdapter.StringStatusToEnum(dataValue);
                    }
                    SensorData.Status = DynamographStatusAdapter.StringStatusToReport(dataValue);
                    return;
                case "BatteryVoltage":
                    //_reportBuilder.BatteryVoltage = dataValue;
                    SensorData.Battery = dataValue;
                    break;
                case "Тemperature":
                    //_reportBuilder.Temperature = dataValue;
                    SensorData.Temperature = dataValue;
                    break;
                case "LoadChanel":
                    _reportBuilder.Load = dataValue;
                    break;
                case "AccelerationChanel":
                    _reportBuilder.Acceleration = dataValue;
                    break;
                case "DeviceProgrammVersion":
                    SensorData.Firmware = dataValue;
                    return;
                case "SensorLoadRKP":
                    _reportBuilder.SensitivityLoad = dataValue;
                    return;
                case "SensorLoadNKP":
                    _reportBuilder.ZeroOffsetLoad = dataValue;
                    return;
                default: return;
            }

            SensorData.Status = _reportBuilder.GetReport();
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
            dataValue = Ddim2.Ddim2Parser.ConvertToStringPayload(resp);
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

            resp = await Connection.Exchange(DynamographCommands.FullCommandDictionary["SensorLoadRKP"]); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = Ddim2.Ddim2Parser.ConvertToStringPayload(resp);
            _reportBuilder.SensitivityLoad = dataValue;

            resp = await Connection.Exchange(DynamographCommands.FullCommandDictionary["SensorLoadNKP"]); ;
            if (0 == resp.Length)
                return false;
            //cmd = pp.DefineCommand(resp);
            dataValue = Ddim2.Ddim2Parser.ConvertToStringPayload(resp);
            _reportBuilder.ZeroOffsetLoad = dataValue;
            return true;

            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadRKP"]);
            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadNKP"]);
        }

        public async Task<bool> CheckStatus()
        {
            byte[] resp;
            //Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();
            //string cmd;
            string dataValue;

            resp = await Connection.Exchange(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]); ;
            if (0 == resp.Length)
                return false;

            //cmd = pp.DefineCommand(resp);
            dataValue = Ddim2.Ddim2Parser.ConvertToStringPayload(resp);
            _measurementManager.MeasurementStatus = DynamographStatusAdapter.StringStatusToEnum(dataValue);
            SensorData.Status = DynamographStatusAdapter.StringStatusToReport(dataValue);

            return true;
            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]);
            //return true;
        }

        public override async Task<bool> PostConnectInit()
        {
            return (await UpdateFirmware() && await KillosParametersQuery());
        }

        public override async Task StartMeasurement(object measurementParameters)
        {
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            //_parser.MessageReceived -= ReceiveHandler;
            SiddosA3MMeasurementStartParameters specificMeasurementParameters =
                (SiddosA3MMeasurementStartParameters)measurementParameters;
            _measurementManager = new SiddosA3MMeasurementManager(Connection, this,
                specificMeasurementParameters);
            //_parser.ExportMemoryFragment += _measurementManager.MemoryRecieveHandler;
            var report = await _measurementManager.RunMeasurement();
            if(null!= report)
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
            //_parser.ExportMemoryFragment -= _measurementManager.MemoryRecieveHandler;
        }

        /*/ Для замера /*/
        private void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            _measurementManager.MeasurementRecieveHandler(commandName, data);
        }

        private void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if(_measurementManager != null)
            {
                _measurementManager.MemoryRecieveHandler(address, data);
                //SensorData.Status = DynamographStatusAdapter.CreateProgressStatus(_measurementManager.Progress);
            }
        }

    }
}
