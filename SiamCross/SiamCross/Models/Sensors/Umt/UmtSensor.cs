using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Umt.Measurement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    public class UmtSensor: ISensor
    {
        private CancellationTokenSource _cancelSource;
        public IBluetoothAdapter BluetoothAdapter { get; }
        private bool _isAlive;
        public bool IsAlive {
            get
            {
                return _isAlive;
            }
            private set
            {
                _isAlive = value;
            }
        }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        //DuMeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; }

        private UmtQuickReportBuilder _reportBuilder;
        private UmtStatusAdapter _statusAdapter;
        private UmtParser _parser;
        private FirmWaveQualifier _firmwareQualifier;
        private Task _liveTask;

        public UmtSensor(IBluetoothAdapter adapter,
                        SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                adapter.SendData,
                UmtCommands.FullCommandDictionary[UmtCommandsEnum.ProgrammVersionAddress],
                UmtCommands.FullCommandDictionary[UmtCommandsEnum.ProgrammVersionSize]
            );
            _parser = new UmtParser(_firmwareQualifier, false);
            _reportBuilder = new UmtQuickReportBuilder();
            _statusAdapter = new UmtStatusAdapter();

            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementBytesReceiveHandler;

            BluetoothAdapter.ConnectSucceed += ConnectHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancelSource = new CancellationTokenSource();
            _liveTask = new Task(async () => await LiveWhile(_cancelSource.Token));
            _liveTask.Start();
        }

        private void MeasurementBytesReceiveHandler(UmtCommandsEnum arg1, byte[] arg2)
        {
            //if (_measurementManager != null)
            //{
            //    _measurementManager.MeasurementRecieveHandler(arg1, arg2);
            //}
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(
                BluetoothAdapter.SendData,
                UmtCommands.FullCommandDictionary[UmtCommandsEnum.ProgrammVersionAddress],
                UmtCommands.FullCommandDictionary[UmtCommandsEnum.ProgrammVersionSize]
            );
            _parser = new UmtParser(_firmwareQualifier, false);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("УМТ успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(UmtCommandsEnum dataName, string dataValue)
        {
            switch (dataName)
            {
                case UmtCommandsEnum.Sostdat:
                    //if (_measurementManager != null)
                    //{
                    //    _measurementManager.MeasurementStatus = _statusAdapter.StringStatusToEnum(dataValue);
                    //}
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    return;
                case UmtCommandsEnum.Acc:
                    _reportBuilder.BatteryVoltage = dataValue;
                    break;
                case UmtCommandsEnum.Dav:
                    _reportBuilder.Pressure = dataValue;
                    break;
                case UmtCommandsEnum.Temp:
                    _reportBuilder.Temperature = dataValue;
                    break;
                case UmtCommandsEnum.DeviceProgrammVersion:
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
            //await BluetoothAdapter.SendData(UmtCommands.FullCommandDictionary[UmtCommandsEnum.Acc]);
            //await Task.Delay(1000);
            //await BluetoothAdapter.SendData(UmtCommands.FullCommandDictionary[UmtCommandsEnum.Temp]);
            //await Task.Delay(1000);
            //await BluetoothAdapter.SendData(UmtCommands.FullCommandDictionary[UmtCommandsEnum.Dav]);
            await BluetoothAdapter.SendData(UmtCommands.FullCommandDictionary
                [UmtCommandsEnum.CurrentParametersFrame]);
            await Task.Delay(500);
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            //IsMeasurement = true;
            //var startParams = (UmtMeasurementStartParameters)measurementParameters;
            //_measurementManager = new UmtMeasurementManager(BluetoothAdapter, SensorData,
            //    startParams);
            //var result = await _measurementManager.RunMeasurement();
            //SensorService.Instance.MeasurementHandler(result);
            //IsMeasurement = false;
        }
    }
}
