using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du
{
    public class DuSensor : BaseSensor
    {
        DuMeasurementManager _measurementManager;
        private DuQuickReportBuilder _reportBuilder=new DuQuickReportBuilder();

        public DuSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
        public async Task<bool> UpdateFirmware(CancellationToken cancelToken)
        {
            byte[] resp;
            byte[] fw_address = new byte[4];
            byte[] fw_size = new byte[2];
            

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange
                (DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionAddress]);
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 4).CopyTo(fw_address);

            cancelToken.ThrowIfCancellationRequested();
            resp = await Connection.Exchange
                (DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionSize]);
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
            //"BatteryVoltage" + "Pressure"

            cancelToken.ThrowIfCancellationRequested();
            byte[] req = DuCommands.FullCommandDictionary[DuCommandsEnum.Voltage];
            byte[] resp = await Connection.Exchange(req);
            if (14 > resp.Length)
                return false;
            SensorData.Battery = (((float)BitConverter.ToInt16(resp, 12)) / 10).ToString();

            cancelToken.ThrowIfCancellationRequested();
            req = DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure];
            resp = await Connection.Exchange(req);
            if (14 > resp.Length)
                return false;
            _reportBuilder.Pressure = (((float)BitConverter.ToInt16(resp, 12)) / 10).ToString();
            
            /*
            byte[] req = new byte[] { 0x0D, 0x0A, 0x01, 0x01,
                0x00, 0x84, 0x00, 0x00,    0x04, 0x00,    0x5C, 0x1C };
            byte[] resp = await Connection.Exchange(req);
            if (16 > resp.Length)
            {
                return false;
            }
            SensorData.Battery = (((float)BitConverter.ToInt16(resp, 12)) / 10).ToString();
            _reportBuilder.Pressure = (((float)BitConverter.ToInt16(resp, 12+2)) / 10).ToString();
            */
            //await mConnection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Voltage]);
            //await mConnection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
            SensorData.Status = _reportBuilder.GetReport();
            return true;

        }
        public override async Task<bool> PostConnectInit(CancellationToken cancelToken)
        {
            SensorData.Status = Resource.ConnectedStatus;
            return (await UpdateFirmware(cancelToken));
        }
        public override async Task StartMeasurement(object measurementParameters)
        {
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            var startParams = (DuMeasurementStartParameters)measurementParameters;
            _measurementManager = new DuMeasurementManager(this, startParams);
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
