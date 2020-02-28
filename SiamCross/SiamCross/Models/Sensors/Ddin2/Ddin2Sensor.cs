using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Services;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Sensor : ISensor
    {
        private Ddin2Parser _parser;
        private Ddin2QuickReportBuiler _reportBuilder;
        private Task _liveTask;
        private CancellationTokenSource _cancellToken =
            new CancellationTokenSource();

        private FirmWaveQualifier _firmwareQualifier;

        public IBluetoothAdapter BluetoothAdapter { get; }

        public SensorData SensorData { get; }

        public bool IsAlive { get; private set; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        public event Action<SensorData> Notify;

        private Ddin2MeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; }

        private Ddin2StatusAdapter _statusAdapter;

        public Ddin2Sensor(IBluetoothAdapter bluetoothAdapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            BluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            _firmwareQualifier = new FirmWaveQualifier(bluetoothAdapter.SendData);
            _parser = new Ddin2Parser(_firmwareQualifier);
            _reportBuilder = new Ddin2QuickReportBuiler();
            _statusAdapter = new Ddin2StatusAdapter();

            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += MessageReceivedHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler; 

            BluetoothAdapter.ConnectSucceed += ConnectSucceedHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancellToken = new CancellationTokenSource();
            _liveTask = new Task(async () => await LiveWhileAsync(_cancellToken.Token));
            _liveTask.Start();
        }

        /*/ Для замера /*/
        private void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            _measurementManager.MeasurementRecieveHandler(commandName, data);
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectSucceedHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(BluetoothAdapter.SendData);
            _parser = new Ddin2Parser(_firmwareQualifier);
            _parser.MessageReceived += MessageReceivedHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("Ддин2 успешно подключен!");
        }

        private async Task LiveWhileAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsAlive)
                {
                    if (SensorData.Firmware == "")
                    {
                        await _firmwareQualifier.Qualify();
                    }
                    if (!_reportBuilder.IsKillosParametersReady)
                    {
                        await KillosParametersQuery();
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
                    Notify?.Invoke(SensorData);

                    await BluetoothAdapter.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            Ddin2MeasurementStartParameters specificMeasurementParameters =
                (Ddin2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddin2MeasurementManager(BluetoothAdapter, SensorData,
                 specificMeasurementParameters);
            var report = await _measurementManager.RunMeasurement();
            SensorService.Instance.MeasurementHandler(report);
            IsMeasurement = false;
        }

        public async Task KillosParametersQuery()
        {
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["SensorLoadRKP"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["SensorLoadNKP"]);
        }

        public async Task CheckStatus()
        {
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["ReadDeviceStatus"]);
        }

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["AccelerationChanel"]);
        }

        private void MessageReceivedHandler(string commandName, string dataValue)
        {
            switch (commandName) // TODO: replace to enum 
            {
                case "DeviceStatus":
                    /*/ Для замера /*/
                    if (_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = _statusAdapter.StringStatusToEnum(dataValue);
                    }
                    Console.WriteLine("Settings Status: " + dataValue);
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    return;
                case "BatteryVoltage":
                    _reportBuilder.BatteryVoltage = dataValue;
                    break;
                case "Тemperature":
                    _reportBuilder.Temperature = dataValue;
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
            Notify?.Invoke(SensorData);
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }
    }
}
