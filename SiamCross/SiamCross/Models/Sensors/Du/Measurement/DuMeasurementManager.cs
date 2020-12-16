using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementManager
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private DuMeasurementStartParameters _measurementParameters;
        private CommandGenerator _commandGenerator;
        private ISensor mSensor;

        public SensorData SensorData { get => mSensor.SensorData; }
        public ISensor Sensor { get => mSensor; }

        public DuMeasurementStatus MeasurementStatus { get; set; }

        byte[] _currentEchogram = new byte[3000];
        private UInt16 mSrcFluidLevel = 0;
        private UInt16 mSrcReflectionsCount = 0;
        private float _pressure;

        private DuParser _parser = new DuParser();

        public DuMeasurementManager(ISensor sensor,
            DuMeasurementStartParameters measurementParameters)
        {
            mSensor = sensor;
            _measurementParameters = measurementParameters;

            _commandGenerator = new CommandGenerator();

            //sensor.Connection.DataReceived += _parser.ByteProcess;
            //_parser.MessageReceived += ReceiveHandler;
            //_parser.ByteMessageReceived += MeasurementRecieveHandler;
        }
        public async Task<object> RunMeasurement()
        {
            MeasureState error = MeasureState.Ok;
            DuMeasurementData report = null;
            try 
            {
                await GetPressure();
                await SendParameters();
                MeasurementStatus = await ExecuteMeasurement();
                if (DuMeasurementStatus.Сompleted != MeasurementStatus)
                {
                    error = MeasureState.LogicError;
                }
                await DownloadHeader();
                await DownloadMeasurement();
                await SetStatusEmpty();
                
            }
            catch(IOEx_Timeout)
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
        private async Task<bool> GetPressure()
        {
            UpdateProgress(_progress, Resource.Init+" "+ Resource.Pressure);
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
            if (14 > resp.Length)
                return false;
            _pressure = (float)BitConverter.ToInt16(resp, 12) / 10.0f;
            Debug.WriteLine("ANNULAR PRESSURE: " + _pressure.ToString());
            return true;
        }
        private async Task SendParameters()
        {
            UpdateProgress(1, Resource.Init);
            Debug.WriteLine("SENDING PARAMETERS");
            BitVector32 myBV = new BitVector32(0);
            int bit0 = BitVector32.CreateMask();
            int bit1 = BitVector32.CreateMask(bit0);
            int bit2 = BitVector32.CreateMask(bit1);
            int bit3 = BitVector32.CreateMask(bit2);
            int bit4 = BitVector32.CreateMask(bit3);
            int bit5 = BitVector32.CreateMask(bit4);
            int bit6 = BitVector32.CreateMask(bit5);
            int bit7 = BitVector32.CreateMask(bit6);
            int bit8 = BitVector32.CreateMask(bit7);
            int bit9 = BitVector32.CreateMask(bit8);

            myBV[bit0] = _measurementParameters.Inlet;
            myBV[bit6] = _measurementParameters.Depth6000;
            myBV[bit9] = _measurementParameters.Amplification;
            byte[] my_data = BitConverter.GetBytes(myBV.Data).AsSpan(0, 2).ToArray();

            byte[] command = _commandGenerator.GenerateWriteCommand(
                DuCommands.FullCommandDictionary[DuCommandsEnum.Revbit], my_data);

            byte[] resp = await Sensor.Connection.Exchange(command);
            if (null == resp || 12 != resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            if (0x02 != resp[3])
                throw new IOEx_ErrorResponse("SetStatus response error");
        }
        private async Task<DuMeasurementStatus> ExecuteMeasurement()
        {
            bool started = await Start();
            if (!started)
                return await GetStatus();

            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            UInt32 measure_time_sec = 40;//18/36
            float sep_cost = 50f / measure_time_sec;
            bool isDone = false;
            for (UInt32 i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                status = await GetStatus();
                switch (status)
                {
                    case DuMeasurementStatus.Сompleted:
                        isDone = true;
                        break;
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                        break;
                }

                _progress += sep_cost;
                UpdateProgress(_progress, DuStatusAdapter.StatusToString(status));
            }
            return status;
        }
        private async Task<bool> DownloadHeader()
        {
            UpdateProgress(_progress, Resource.ReadingHeader);
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.ResearchHeader]);
            if (16 > resp.Length)
                return false;
                
            mSrcFluidLevel = BitConverter.ToUInt16(resp, 12);
            // ахтунг! параметр передаётся в двоично десятичном виде :-(
            mSrcReflectionsCount = BitConverter.ToUInt16(resp, 14);
            return true;
        }
        private DuMeasurementData MakeReport(MeasureState state)
        {
            _logger.Trace("begin create report");

            var data = new DuMeasurementData(DateTime.Now
                , _measurementParameters
                , _pressure
                , mSrcFluidLevel
                , mSrcReflectionsCount
                , _currentEchogram
                , state);

            _logger.Trace("end create report");

            return data;
        }
        static private async Task<bool> ReadMemory(IProtocolConnection conn, byte[] dst, int start
            , UInt32 start_addr, UInt32 mem_size, UInt16 step_size
            , Action<float> DoStepProgress, float progress_size)
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
                    curr_addr += mem_size - curr_addr;
            }

            return true;
        }
        private async Task DownloadMeasurement()
        {
            _logger.Trace("begin read echogramm");
            float progress_size = (100f - _progress) ;

            UpdateProgress(_progress, Resource.Downloading);
            Action<float> StepProgress = (float sep_cost) =>
            {
                _progress += sep_cost;
                UpdateProgress(_progress);
            };
            await ReadMemory(Sensor.Connection, _currentEchogram
                , 0, 0x81000000, (uint)_currentEchogram.Length , 50, StepProgress, progress_size);

            _logger.Trace("end read echogramm");
        }
        private async Task<bool> Start()
        {
            UpdateProgress(_progress, Resource.start);
            System.Diagnostics.Debug.WriteLine("Execute start measure");
            await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StartMeasurement]);
            await Task.Delay(Constants.SecondDelay);
            UInt32 time_sec = 30;
            float sep_cost = 10f / time_sec;
            bool isDone = false;
            for (UInt32 i = 0; i < time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                DuMeasurementStatus status = await GetStatus();
                switch (status)
                {
                    case DuMeasurementStatus.WaitingForClick:
                        isDone = true;
                        break;
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                    case DuMeasurementStatus.Сompleted:
                        break;
                }
                _progress += sep_cost;
                UpdateProgress(_progress, DuStatusAdapter.StatusToString(status));
            }
            return isDone;

        }
        private async Task<DuMeasurementStatus> GetStatus()
        {
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);
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
        private async Task SetStatusEmpty()
        {
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StateZeroing]);
            if (null == resp || 12 != resp.Length)
                throw new IOEx_Timeout("SetStatus wrong len or timeout");
            if (0x02 != resp[3])
                throw new IOEx_ErrorResponse("SetStatus response error");
        }
        private void UpdateProgress(float pos)
        {
            _progress = pos;
            Sensor.MeasureProgress = _progress / 100;
        }
        private void UpdateProgress(float pos, string text)
        {
            SensorData.Status = Resource.Survey+": " + text;
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
  
    }
}
