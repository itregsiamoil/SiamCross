using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du
{
    public class DuSensor : BaseSensor
    {
        private DuMeasurementManager _measurementManager;
        private readonly DuQuickReportBuilder _reportBuilder = new DuQuickReportBuilder();

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
            byte[] req = new MessageCreator().CreateReadMessage(fw_address, fw_size);
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



        private async Task<DuMeasurementStatus> GetStatus(CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            resp = await Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);
            if (null == resp || 12 > resp.Length)
                throw new IOEx_Timeout("GetStatus timeout");
            if (0x01 != resp[3])
                throw new IOEx_ErrorResponse("GetStatus response error");
            if (16 != resp.Length)
                throw new IOEx_ErrorResponse("GetStatus response length error");
            status = (DuMeasurementStatus)BitConverter.ToUInt16(resp, 12);
            System.Diagnostics.Debug.WriteLine("DU status=" + status.ToString());
            return status;
        }
        private async Task SetStatusEmpty(CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            byte[] resp = { };
            resp = await Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StateZeroing]);
            if (null == resp || 12 != resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            if (0x02 != resp[3])
                throw new IOEx_ErrorResponse("SetStatus response error");
        }
        public override async Task<bool> PostConnectInit(CancellationToken cancelToken)
        {
            // датчик в режиме измерения отвечает ТОЛЬКО на опрос состояния 
            // и не отвечает на другие команды следовательно, 
            // чтоб подключиться к нему:
            // 1 опрашиваем статус
            DuMeasurementStatus status = await GetStatus(cancelToken);
            // 2 делаем сброс если занят
            cancelToken.ThrowIfCancellationRequested();
            switch (status)
            {
                case DuMeasurementStatus.Empty:
                    break;
                default:
                case DuMeasurementStatus.WaitingForClick:
                case DuMeasurementStatus.EсhoMeasurement:
                case DuMeasurementStatus.NoiseMeasurement:
                case DuMeasurementStatus.Сompleted:
                    await SetStatusEmpty(cancelToken);
                    break;
            }
            // 3 проверяем прошивку
            return (await UpdateFirmware(cancelToken));
        }
        public override async Task StartMeasurement(object measurementParameters)
        {
            object report = null;
            try
            {
                SensorData.Status = Resource.Survey;
                IsMeasurement = true;
                DuMeasurementStartParameters startParams = (DuMeasurementStartParameters)measurementParameters;
                _measurementManager = new DuMeasurementManager(this, startParams);
                report = await _measurementManager.RunMeasurement();

            }
            catch (Exception)
            {
                report = null;
            }
            finally
            {
                if (null != report)
                {
                    SensorService.Instance.MeasurementHandler(report);
                    SensorData.Status = Resource.Survey + ": complete";
                }
                else
                {
                    SensorData.Status = Resource.Survey + ": " + Resource.Error;
                }
                await Task.Delay(2000);
                IsMeasurement = false;
            }
        }
    }
}
