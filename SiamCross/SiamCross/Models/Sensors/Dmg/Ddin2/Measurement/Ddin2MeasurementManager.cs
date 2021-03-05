using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.Ddin2.Measurement
{
    public class Ddin2MeasurementManager
    {
        private readonly Ddin2MeasurementStartParameters _measurementParameters;
        private DmgBaseMeasureReport _report;
        private readonly DmgBaseSensor mSensor;
        private readonly IProtocolConnection _Connection;
        private readonly Stopwatch _PerfCounter = new Stopwatch();

        public SensorData SensorData => mSensor.SensorData;
        public ISensor Sensor => mSensor;
        public UInt32 ErrorCode { get; private set; }

        private readonly byte[] _currentDynGraph = new byte[1000 * 2];
        private readonly byte[] _currentAccelerationGraph = new byte[1000 * 2];

        public Ddin2MeasurementManager(ISensor sensor,
             Ddin2MeasurementStartParameters measurementParameters)
        {
            _measurementParameters = measurementParameters;
            mSensor = sensor as DmgBaseSensor;
            _Connection = mSensor.Connection;
        }

        public async Task<object> RunMeasurement()
        {
            _PerfCounter.Restart();
            MeasureState error = MeasureState.Ok;
            DmgMeasureStatus MeasurementStatus = DmgMeasureStatus.Empty;
            Ddin2MeasurementData report;
            try
            {
                await SendParameters();
                bool started = await Start();
                if (started)
                {
                    while (DmgMeasureStatus.Ready != MeasurementStatus
                        && _PerfCounter.ElapsedMilliseconds < 300 * 1000)
                    {
                        _progress = 1;
                        MeasurementStatus = await ExecuteMeasurement();
                    }
                }

                if (DmgMeasureStatus.Ready != MeasurementStatus)
                {
                    ErrorCode = await ReadErrorCode();
                    if (0 < ErrorCode)
                        error = MeasureState.LogicError;
                }
                await DownloadHeader();
                await DownloadMeasurement();
                await SetStatusEmpty();

            }
            catch (ProtocolException)
            {
                Debug.WriteLine("RunMeasurement IO Error");
                error = MeasureState.IOError;
            }
            catch (Exception)
            {
                Debug.WriteLine("RunMeasurement Unknown Error");
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
        public int Progress => (int)_progress;

        private async Task SendParameters()
        {
            UpdateProgress(1, Resource.Init);

            mSensor.Rod.Value = (UInt16)_measurementParameters.Rod;
            mSensor.DynPeriod.Value = (UInt32)_measurementParameters.DynPeriod;
            mSensor.ApertNumber.Value = (UInt16)_measurementParameters.ApertNumber;
            mSensor.Imtravel.Value = (UInt16)_measurementParameters.Imtravel;
            mSensor.ModelPump.Value = (UInt16)_measurementParameters.ModelPump;

            if (10 > mSensor.MemoryModelVersion.Value)
            {
                await _Connection.WriteAsync(mSensor.Rod);
                await _Connection.WriteAsync(mSensor.DynPeriod);
                await _Connection.WriteAsync(mSensor.ApertNumber);
                await _Connection.WriteAsync(mSensor.Imtravel);
                await _Connection.WriteAsync(mSensor.ModelPump);
            }
            else
            {
                await _Connection.WriteAsync(mSensor._SurvayParam);
            }
            Debug.WriteLine("PARAMETERS HAS BEEN SENT");
        }

        private async Task<DmgMeasureStatus> ExecuteMeasurement()
        {

            DmgMeasureStatus status = DmgMeasureStatus.Empty;
            UInt32 dyn_period = mSensor.DynPeriod.Value;

            const UInt32 calc_time_sec = 10;
            UInt32 measure_time_sec = dyn_period * 5 / 1000 + calc_time_sec;
            float sep_cost = 50f / measure_time_sec;

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
            mSensor.CtrlReg.Value = 0x01;
            RespResult ret = await _Connection.WriteAsync(mSensor.CtrlReg);
            Debug.WriteLine("MEASUREMENT STARTED");
            return RespResult.NormalPkg == ret;
        }

        public async Task<UInt32> ReadErrorCode()
        {
            await _Connection.ReadAsync(mSensor.ErrorReg);
            return mSensor.ErrorReg.Value;
        }
        private async Task<DmgMeasureStatus> GetStatus()
        {
            await _Connection.ReadAsync(mSensor.StatReg);
            Debug.WriteLine($"Status is {(DmgMeasureStatus)mSensor.StatReg.Value}");
            return (DmgMeasureStatus)mSensor.StatReg.Value;
        }

        public async Task<bool> DownloadHeader()
        {
            Debug.WriteLine("DownloadHeader start");
            UpdateProgress(_progress, Resource.ReadingHeader);
            await _Connection.ReadAsync(mSensor._Report);
            _report = new DmgBaseMeasureReport(
                mSensor.MaxWeight.Value
                , mSensor.MinWeight.Value
                , mSensor.Travel.Value
                , mSensor.Period.Value
                , mSensor.Step.Value
                , mSensor.WeightDiscr.Value
                , mSensor.TimeDiscr.Value);
            Debug.WriteLine("DownloadHeader end");
            return true;
        }




        public async Task DownloadMeasurement()
        {
            Debug.WriteLine("DownloadMeasurement start");
            UpdateProgress(_progress, Resource.Downloading);


            float global_progress_start = _progress;
            float global_progress_left = (100f - _progress);
            Action<float> StepProgress = (float progress) =>
            {
                _progress = global_progress_start + progress * global_progress_left;
                UpdateProgress(_progress);
            };

            RespResult ret = await _Connection.ReadMemAsync(0x81000000, 1000 * 2, _currentDynGraph
                , 0, StepProgress);

            Debug.WriteLine(" elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString());


            /*
            UpdateProgress(_progress, "Read axgm");
            await ReadMemory(Sensor.Connection, _currentAccelerationGraph
                , 0, 0x83000000, 1000 * 2, 50, StepProgress, progress_size);
            */
            Debug.WriteLine("DownloadMeasurement end");
        }

        private Ddin2MeasurementData MakeReport(MeasureState state)
        {
            Debug.WriteLine("MakeReport start");

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
                    BitConverter.GetBytes(ErrorCode));

            double[,] dynGraphPoints = DgmConverter.GetXYs(measurement.DynGraph.ToList(),
                measurement.Report.Step, measurement.Report.WeightDiscr);
            measurement.DynGraphPoints = dynGraphPoints;

            Debug.WriteLine("MakeReport end");
            return measurement;
        }

        private async Task SetStatusEmpty()
        {
            mSensor.CtrlReg.Value = 0x02;
            await _Connection.WriteAsync(mSensor.CtrlReg);
            Debug.WriteLine("SetStatusEmpty end");
        }

    }
}
