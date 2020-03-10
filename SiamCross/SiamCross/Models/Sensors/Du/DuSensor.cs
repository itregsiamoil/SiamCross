using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Du.Measurement;
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

        public event Action<SensorData> Notify;

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
            _parser.ByteMessageReceived += MeasurementRecieveHandler;

            BluetoothAdapter.ConnectSucceed += ConnectHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancelSource = new CancellationTokenSource();
            _liveTask = new Task(async () => await LiveWhile(_cancelSource.Token));
            _liveTask.Start();
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
                        //
                    }
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    break;
                case DuCommandsEnum.Voltage:
                    _reportBuilder.BatteryVoltage = dataValue;
                    break;
                case DuCommandsEnum.Pressure:
                    _reportBuilder.Pressure = dataValue;
                    break;
                case DuCommandsEnum.DeviceProgrammVersion:
                    SensorData.Firmware = dataValue;
                    break;
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
                        await Task.Delay(1500);
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
            await BluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
        }

        public Task StartMeasurement(object measurementParameters)
        {
            throw new NotImplementedException();
        }

        private void MeasurementRecieveHandler(DuCommandsEnum commandName, byte[] data)
        {
            //_measurementManager.MeasurementRecieveHandler(commandName, data);
        }
    }
}
