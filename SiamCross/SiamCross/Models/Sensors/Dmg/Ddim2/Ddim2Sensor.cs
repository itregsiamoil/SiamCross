using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg.Ddim2.Measurement;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using SiamCross.Services;

namespace SiamCross.Models.Sensors.Dmg.Ddim2
{
    public class Ddim2Sensor : ISensor
    {
        public float MeasureProgress { get; set; }

        protected IProtocolConnection mConnection;
        public IProtocolConnection Connection => mConnection;

        private CancellationTokenSource _cancellToken;
        public bool IsAlive { get; private set; }

        private bool _activated=false;
        public bool Activate
        {
            get => _activated;
            set
            {
                _activated = value;
                if(_activated)
                    _liveTask.Start();
                else
                    _cancellToken.Cancel();
            }
        }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        Ddim2MeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; } 

        private Ddim2QuickReportBuilder _reportBuilder;
        private Ddim2Parser _parser;
        private FirmWaveQualifier _firmwareQualifier;

        private Task _liveTask;
        public Ddim2Sensor(IProtocolConnection adapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            mConnection = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                adapter.SendData,
                DmgCmd.Get("ProgrammVersionAddress"),
                DmgCmd.Get("ProgrammVersionSize")
            );
            _parser = new Ddim2Parser(_firmwareQualifier, true);
            _reportBuilder = new Ddim2QuickReportBuilder();

            mConnection.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;
            _parser.ExportMemoryFragment += MemoryRecieveHandler;

            mConnection.ConnectSucceed += ConnectHandler;
            mConnection.ConnectFailed += ConnectFailedHandler;

            _cancellToken = new CancellationTokenSource();
            _liveTask = new Task(async () => await ExecuteAsync(_cancellToken.Token));
            //_liveTask.Start();
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(
                mConnection.SendData,
                DmgCmd.Get("ProgrammVersionAddress"),
                DmgCmd.Get("ProgrammVersionSize")
            );
            _parser = new Ddim2Parser(_firmwareQualifier, true);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("Ддим2 успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(string dataName, string dataValue)
        {
            switch (dataName)
            {
                case "DeviceStatus":
                    /*/ Для замера /*/
                    if (_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = DmgMeasureStatusAdapter.StringStatusToEnum(dataValue);
                    }
                    SensorData.Status = DmgMeasureStatusAdapter.StringStatusToReport(dataValue);
                    return;
                case "BatteryVoltage":
                    //_reportBuilder.BatteryVoltage = dataValue;
                    SensorData.Battery = dataValue;
                    break;
                case "Тemperature":
                    //_reportBuilder.Temperature = dataValue;
                    SensorData.Temperature = dataValue;
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
        }

        public async Task<bool> QuickReport()
        {
            await mConnection.SendData(DmgCmd.Get("BatteryVoltage"));
            await mConnection.SendData(DmgCmd.Get("Тemperature"));
            await mConnection.SendData(DmgCmd.Get("LoadChanel"));
            await mConnection.SendData(DmgCmd.Get("AccelerationChanel"));
            return true;
        }

        public async Task KillosParametersQuery()
        {
            await mConnection.SendData(DmgCmd.Get("SensorLoadRKP"));
            await mConnection.SendData(DmgCmd.Get("SensorLoadNKP"));
        }

        public async Task CheckStatus()
        {
            await mConnection.SendData(DmgCmd.Get("ReadDeviceStatus"));
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            while(!cancellationToken.IsCancellationRequested)
            {
                if(IsAlive)
                {
                    if(SensorData.Firmware == "")
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

                    await mConnection.Connect();
                    await Task.Delay(1000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            _parser.MessageReceived -= ReceiveHandler;
            Ddim2MeasurementStartParameters specificMeasurementParameters = 
                (Ddim2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddim2MeasurementManager(mConnection, SensorData, 
                specificMeasurementParameters);
            var report = await _measurementManager.RunMeasurement();
            SensorService.Instance.MeasurementHandler(report);
            IsMeasurement = false;
        }

        /*/ Для замера /*/
        private void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            _measurementManager.MeasurementRecieveHandler(commandName, data);
        }

        private void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if (_measurementManager != null)
            {
                _measurementManager.MemoryRecieveHandler(address, data);
                SensorData.Status = DmgMeasureStatusAdapter.CreateProgressStatus(_measurementManager.Progress);             
            }
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            mConnection.Disconnect();
        }
    }
}
