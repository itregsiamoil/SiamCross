using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2.Measurement;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Sensor : ISensor
    {
        private Ddin2Parser _parser;
        private Ddin2QuickReportBuiler _reportBuilder;
        private Task _liveTask;
        private CancellationTokenSource _cancellToken =
            new CancellationTokenSource();

        public IBluetoothAdapter BluetoothAdapter { get; }

        public SensorData SensorData { get; }

        public bool IsAlive { get; private set; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        public event Action<SensorData> Notify;

        private Ddin2MeasurementManager _measurementManager;

        public bool IsMeasurement { get; set; }

        private Ddin2StatusAdapter _statusAdapter;

        public Ddin2Sensor(IBluetoothAdapter bluetoothAdapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            BluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            _parser = new Ddin2Parser();
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
            _parser = new Ddin2Parser();
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
                    if (!IsMeasurement)
                    {
                        await QuickReport();
                        await Task.Delay(1500);
                    }
                    else
                    {

                    }
                    
                }
                else
                {
                    SensorData.Status = "Нет связи";
                    Notify?.Invoke(SensorData);

                    await BluetoothAdapter.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            //_cancellToken.Cancel();
            //_parser.MessageReceived -= MessageReceivedHandler;
            Ddin2MeasurementStartParameters specificMeasurementParameters =
                (Ddin2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddin2MeasurementManager(BluetoothAdapter, SensorData,
                _parser, specificMeasurementParameters, this);
            await _measurementManager.RunMeasurement();
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
                    _measurementManager.MeasurementStatus = _statusAdapter.StringStatusToEnum(dataValue);
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
