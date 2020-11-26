using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du
{
    public class DuSensor : ISensor
    {
        public float MeasureProgress { get; set; }

        protected IProtocolConnection mConnection;
        public IProtocolConnection Connection => mConnection;

        private CancellationTokenSource _cancellToken;
        
        private bool _activated = false;
        public bool Activate
        {
            get => _activated;
            set
            {
                _activated = value;
                if (_activated)
                    _liveTask.Start();
                else
                    _cancellToken.Cancel();
            }
        }
        public bool IsAlive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        DuMeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; }

        private DuQuickReportBuilder _reportBuilder;
        private DuStatusAdapter _statusAdapter;
        private DuParser _parser;
        private FirmWaveQualifier _firmwareQualifier;
        private Task _liveTask;

        public DuSensor(IProtocolConnection adapter, 
                        SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            mConnection = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                adapter.SendData,
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionAddress],
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionSize]
            );
            _parser = new DuParser(_firmwareQualifier, false);
            _reportBuilder = new DuQuickReportBuilder();
            _statusAdapter = new DuStatusAdapter();

            mConnection.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementBytesReceiveHandler;

            mConnection.ConnectSucceed += ConnectHandler;
            mConnection.ConnectFailed += ConnectFailedHandler;

            _cancellToken = new CancellationTokenSource();
            _liveTask = new Task(async () => await LiveWhile(_cancellToken.Token));
            //_liveTask.Start();
        }

        private void MeasurementBytesReceiveHandler(DuCommandsEnum arg1, byte[] arg2)
        {
            if (_measurementManager != null)
            {
                _measurementManager.MeasurementRecieveHandler(arg1, arg2);
                if (arg1 != DuCommandsEnum.Pressure)
                {
                    SensorData.Status = _statusAdapter.CreateProgressStatus(_measurementManager.Progress);
                }
            }
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(
                mConnection.SendData,
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionAddress],
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionSize]
            );
            _parser = new DuParser(_firmwareQualifier, false);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("ДУ успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(DuCommandsEnum dataName, string dataValue)
        {
            switch(dataName)
            {
                case DuCommandsEnum.SensorState:
                    if(_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = _statusAdapter.StringStatusToEnum(dataValue);
                    }
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    return;
                case DuCommandsEnum.Voltage:
                    //_reportBuilder.BatteryVoltage = dataValue;
                    SensorData.Battery = dataValue;
                    break;
                case DuCommandsEnum.Pressure:
                    _reportBuilder.Pressure = dataValue;
                    break;
                case DuCommandsEnum.DeviceProgrammVersion:
                    SensorData.Firmware = dataValue;
                    break;
                default:
                    return;
            }

            SensorData.Status = _reportBuilder.GetReport();
        }

        private async Task LiveWhile(CancellationToken token)
        { 
            await Task.Delay(2000);
            while (!token.IsCancellationRequested)
            {
                if (IsAlive)
                {
                    if (SensorData.Firmware == "")
                    {
                        await _firmwareQualifier.Qualify();
                    }
                    if (!IsMeasurement)
                    {
                        await QuickReport();
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    SensorData.Status = Resource.NoConnection;

                    await mConnection.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            mConnection.Disconnect();
        }

        public async Task<bool> QuickReport()
        {
            await mConnection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Voltage]);
            await Task.Delay(300);
            await mConnection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
            return true;
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            var startParams = (DuMeasurementStartParameters)measurementParameters;
            _measurementManager = new DuMeasurementManager(mConnection, SensorData,
                startParams);
            var result = await _measurementManager.RunMeasurement();
            SensorService.Instance.MeasurementHandler(result);
            IsMeasurement = false;
        }   
    }
}
