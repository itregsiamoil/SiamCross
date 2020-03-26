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
        private CancellationTokenSource _cancelSource;
        public IBluetoothAdapter BluetoothAdapter { get; }
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

        public DuSensor(IBluetoothAdapter adapter, 
                        SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                adapter.SendData,
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionAddress],
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionSize]
            );
            _parser = new DuParser(_firmwareQualifier);
            _reportBuilder = new DuQuickReportBuilder();
            _statusAdapter = new DuStatusAdapter();

            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementBytesReceiveHandler;

            BluetoothAdapter.ConnectSucceed += ConnectHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancelSource = new CancellationTokenSource();
            _liveTask = new Task(async () => await LiveWhile(_cancelSource.Token));
            _liveTask.Start();
        }

        private void MeasurementBytesReceiveHandler(DuCommandsEnum arg1, byte[] arg2)
        {
            if (_measurementManager != null)
            {
                _measurementManager.MeasurementRecieveHandler(arg1, arg2);
            }
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(
                BluetoothAdapter.SendData,
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionAddress],
                DuCommands.FullCommandDictionary[DuCommandsEnum.ProgrammVersionSize]
            );
            _parser = new DuParser(_firmwareQualifier);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("ДУ успешно подключен!");
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
                    _reportBuilder.BatteryVoltage = dataValue;
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

                    await BluetoothAdapter.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
            BluetoothAdapter.Disconnect();
        }

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Voltage]);
            await Task.Delay(300);
            await BluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            var startParams = (DuMeasurementStartParameters)measurementParameters;
            _measurementManager = new DuMeasurementManager(BluetoothAdapter, SensorData,
                startParams);
            var result = await _measurementManager.RunMeasurement();
            SensorService.Instance.MeasurementHandler(result);
            IsMeasurement = false;
        }   
    }
}
