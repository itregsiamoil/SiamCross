using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.Ddin2.Measurement
{
    public class Ddin2MeasurementManager
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private CommandGenerator _configGenerator;
        private Ddin2MeasurementStartParameters _measurementParameters;
        private DmgBaseMeasureReport _report;
        private ISensor mSensor;

        public SensorData SensorData { get => mSensor.SensorData; }
        public ISensor Sensor { get => mSensor; }
        public byte[] ErrorCode { get; private set; }
        public DmgMeasureStatus MeasurementStatus { get; set; }

        byte[] _currentDynGraph = new byte[1000*2];
        byte[] _currentAccelerationGraph = new byte[1000 * 2];

        public Ddin2MeasurementManager(ISensor sensor,
             Ddin2MeasurementStartParameters measurementParameters)
        {
            _measurementParameters = measurementParameters;
            _configGenerator = new CommandGenerator();
            mSensor = sensor;

            //_currentDynGraph = new List<byte[]>();
            //_currentAccelerationGraph = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            MeasureState error = MeasureState.Ok;
            Ddin2MeasurementData report = null;
            try
            {
                await SendParameters();
                MeasurementStatus = await ExecuteMeasurement();
                if (DmgMeasureStatus.Ready != MeasurementStatus)
                {
                    error = MeasureState.LogicError;
                    ErrorCode = await ReadErrorCode();
                }
                await DownloadHeader();
                await DownloadMeasurement();
                await SetStatusEmpty();

            }
            catch (IOEx_Timeout)
            {
                error = MeasureState.IOError;
            }
            catch (IOEx_ErrorResponse)
            {
                error = MeasureState.LogicError;
            }
            catch (Exception)
            {
                error = MeasureState.UnknownError;
            }
            finally
            {
                report = MakeReport(error);
            }
            return report;
        }

        private void UpdateProgress(float pos)
        {
            _progress = pos;
            Sensor.MeasureProgress = _progress / 100;
        }

        private void UpdateProgress(float pos, string text)
        {
            SensorData.Status = Resource.Survey + ": " + text;
            UpdateProgress(pos);
        }
        private float _progress = 0;
        public int Progress
        {
            get
            {
                return (int)_progress;
            }
        }

        private async Task SendParameters()
        {
            UpdateProgress(1, Resource.Init);
            Console.WriteLine("SENDING PARAMETERS");
            byte[] resp = { };
            //Console.WriteLine("SetDynPeriod: " + BitConverter.ToString(_configGenerator.SetDynPeriod(_measurementParameters.DynPeriod)));
            resp = await Sensor.Connection.Exchange(_configGenerator.SetDynPeriod(_measurementParameters.DynPeriod));
            if (0 == resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            //Console.WriteLine("SetApertNumber: " + BitConverter.ToString(_configGenerator.SetApertNumber(_measurementParameters.ApertNumber)));
            resp = await Sensor.Connection.Exchange(_configGenerator.SetApertNumber(_measurementParameters.ApertNumber));
            if (0 == resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            //Console.WriteLine("SetImtravel", _configGenerator.SetImtravel(_measurementParameters.Imtravel));
            resp = await Sensor.Connection.Exchange(_configGenerator.SetImtravel(_measurementParameters.Imtravel));
            if (0 == resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            //Console.WriteLine("SetModelPump", _configGenerator.SetModelPump(_measurementParameters.ModelPump));
            resp = await Sensor.Connection.Exchange(_configGenerator.SetModelPump(_measurementParameters.ModelPump));
            if (0 == resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            //Console.WriteLine("SetRod: " + BitConverter.ToString(_configGenerator.SetRod(_measurementParameters.Rod)));
            resp = await Sensor.Connection.Exchange(_configGenerator.SetRod(_measurementParameters.Rod));
            if (0 == resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            Console.WriteLine("PARAMETERS HAS BEEN SENT");
        }

        private async Task<DmgMeasureStatus> ExecuteMeasurement()
        {
            bool started = await Start();
            if (!started)
                return await GetStatus();


            DmgMeasureStatus status = DmgMeasureStatus.Empty;
            byte[] resp = { };

            resp = await Sensor.Connection.Exchange(DmgCmd.Get("DynPeriod"));
            if (0 == resp.Length)
                return DmgMeasureStatus.Error;
            UInt32 dyn_period = BitConverter.ToUInt32(resp, 12);

            const UInt32 calc_time_sec = 10;
            UInt32 measure_time_sec = dyn_period * 5 / 1000 + calc_time_sec;
            float sep_cost = 50f/ measure_time_sec;

            bool isDone = false;
            for (UInt32 i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);

                status = await GetStatus();
                if (status == DmgMeasureStatus.Ready
                    || status == DmgMeasureStatus.Error)
                {
                    isDone = true;
                }
                _progress += sep_cost;
                UpdateProgress(_progress, DmgMeasureStatusAdapter.StatusToReport(status));
            }
            return status;
        }

        private async Task<bool> Start()
        {
            UpdateProgress(2, Resource.start);
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DmgCmd.Get("StartMeasurement"));
            if (0 == resp.Length)
                return false;
            Console.WriteLine("MEASUREMENT STARTED");
            return true;
        }

        public async Task<byte[]> ReadErrorCode()
        {
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DmgCmd.Get("ReadMeasurementErrorCode"));
            if (0 == resp.Length)
            {
                Console.WriteLine("Can`t read error code");
                return new byte[] { };
            }
            return resp.AsSpan().Slice(12, 2).ToArray();
        }
        private async Task<DmgMeasureStatus> GetStatus()
        {
            DmgMeasureStatus status = DmgMeasureStatus.Empty;
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DmgCmd.Get("ReadDeviceStatus"));
            if (null == resp || 12 > resp.Length)
                throw new IOEx_Timeout("GetStatus timeout");
            if (0x01 != resp[3])
                throw new IOEx_ErrorResponse("GetStatus response error");
            if (16 != resp.Length)
                throw new IOEx_ErrorResponse("GetStatus response length error");
            status = (DmgMeasureStatus)BitConverter.ToUInt16(resp, 12);
            System.Diagnostics.Debug.WriteLine("status=" + status.ToString());
            return status;
        }

        public async Task<bool> DownloadHeader()
        {
            UpdateProgress(_progress, Resource.ReadingHeader);
            int retry = 3;
            bool is_ok = false;
            for (int r = 0; r < retry && !is_ok; r++)
            {
                byte[] message = await Sensor.Connection.Exchange(DmgCmd.Get("ReadMeasurementReport"));
                if (null == message || 0 == message.Length)
                    continue;

                var data = Ddin2Parser.GetPayload(message);
                var report = new List<UInt16>();
                for (int i = 0; i + 1 < data.Count(); i += 2)
                {
                    var array = new byte[] { data[i], data[i + 1] };
                    UInt16 value = BitConverter.ToUInt16(array, 0);
                    report.Add(value);
                }
                _report = new DmgBaseMeasureReport(
                    report[0], report[1], report[2], report[3],
                    report[4], report[5], report[6]);
                is_ok = true;
            }//for
            return is_ok;
        }

        public async Task DownloadMeasurement()
        {
            UpdateProgress(_progress, Resource.Downloading);
            float progress_size = (100f - _progress);

            Action<float> StepProgress = (float sep_cost) =>
            {
                _progress += sep_cost;
                UpdateProgress(_progress);
            };
            await ReadMemory(Sensor.Connection, _currentDynGraph
                , 0, 0x81000000, 1000 * 2, 50, StepProgress, progress_size);

            /*
            UpdateProgress(_progress, "Read axgm");
            await ReadMemory(Sensor.Connection, _currentAccelerationGraph
                , 0, 0x83000000, 1000 * 2, 50, StepProgress, progress_size);
            */
        }

        private Ddin2MeasurementData MakeReport(MeasureState state)
        {
            _logger.Trace("begin create report");
            List<byte> dynRawBytes = _currentDynGraph.ToList();

            Ddin2MeasurementData measurement =
                new Ddin2MeasurementData(
                    _report,
                    (short)_measurementParameters.ApertNumber,
                    (short)_measurementParameters.ModelPump,
                    (short)_measurementParameters.Rod,
                    _currentDynGraph.ToList(),
                    DateTime.Now,
                    _measurementParameters.SecondaryParameters,
                    _currentAccelerationGraph.ToList(),
                    ErrorCode);   
            _logger.Trace("end create report");
            var dynGraphPoints = DgmConverter.GetXYs(measurement.DynGraph.ToList(),
                measurement.Report.Step, measurement.Report.WeightDiscr);
            measurement.DynGraphPoints = dynGraphPoints;
            return measurement;
        }

        static private async Task<bool> ReadMemory(IProtocolConnection conn, byte[] dst,int start
            , UInt32 start_addr, UInt32 mem_size, UInt16 step_size
            , Action<float> DoStepProgress,  float progress_size)
        {
            if (null == dst || dst.Length < (int)mem_size)
                throw new Exception("dst is too short");
            
            //const UInt32 start_addr = 0x81000000;
            //const UInt32 mem_size = 1000*2;
            //const UInt32 step_size = 20;
            UInt32 step_count = mem_size / step_size;
            
            float sep_cost = (progress_size) / step_count;

            byte[] cmd = new byte[]
            {
                0x0D, 0x0A, //header
                0x01, 0x01, // device // command
                0x00, 0x00, 0x00, 0x81, // address
                0x00, 0x00, // length
                0xFF, 0xFF, // crc

            };
            byte[] resp = { };
            UInt32 curr_addr = 0;

            while (mem_size > curr_addr)
            {
                UInt32 addr = curr_addr + start_addr;
                BitConverter.GetBytes(addr).CopyTo(cmd.AsSpan(4, 4)); //addr
                BitConverter.GetBytes(step_size).CopyTo(cmd.AsSpan(8, 2)); //len
                byte[] crc = CrcModbusCalculator.ModbusCrc(cmd, 2, cmd.Length - 4);
                crc.CopyTo(cmd.AsSpan(10, 2)); //crc

                resp = await conn.Exchange(cmd);
                if (0 == resp.Length)
                    return false;

                resp.AsSpan(12, (int)step_size).CopyTo(dst.AsSpan((int)curr_addr, (int)step_size));
                DoStepProgress(sep_cost);

                UInt32 next_curr_addr = curr_addr + step_size;
                if (mem_size > next_curr_addr)
                    curr_addr = next_curr_addr;
                else
                    curr_addr += mem_size- curr_addr;
            }

            return true;
        }
        private async Task SetStatusEmpty()
        {
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DmgCmd.Get("InitializeMeasurement"));
            if (null == resp || 12 != resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            if (0x02 != resp[3])
                throw new IOEx_ErrorResponse("SetStatus response error");
        }

    }
}
