using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SiamCross.Models.Sensors;
using SiamCross.Models.Tools;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M
{
    public class SiddosA3MSensor : ISensor
    {
        
        public IBluetoothAdapter BluetoothAdapter { get; }

        public async Task PollAsync()
        {
            await ExecuteAsync(_cancellToken.Token);
        }

        private void ClearStatus() 
        { 
                IsAlive = false;
                SensorData.Temperature = "";
                SensorData.Battery = "";
                SensorData.Firmware = "";
                SensorData.RadioFirmware = "";
                SensorData.Status = Resource.NoConnection;        
        }
        
        TaskScheduler _uiScheduler;
        async void AsyncActivate()
        {
            //_activated = await AsyncPoll2();
            _cancellToken = new CancellationTokenSource();
            _liveTask = Task.Run(async () =>
            {
                await PollAsync();
                return false;
            }, _cancellToken.Token);
            _activated = true;
            try
            {
                _activated = await _liveTask;
            }
            catch (OperationCanceledException)
            {
                SensorData.Status = "close connection";
                Debug.WriteLine($"Cancel liveupdate");
            }
            finally
            {
                _liveTask.Dispose();
                _cancellToken.Dispose();
                _liveTask = null;
                _cancellToken = null;
                _activated = false;
                await BluetoothAdapter.Disconnect();
                ClearStatus();
            }
        }
        private CancellationTokenSource _cancellToken;
        private Task<bool> _liveTask;
        private bool _activated = false;
        public bool Activeted
        {
            get => _activated;
            set
            {
                //_activated = value;
                if (value && !_activated && null==_liveTask)
                {
                    AsyncActivate();
                }
                else
                {
                    if(null != _cancellToken)
                        _cancellToken.Cancel();
                }
                    
            }
        }
        public bool IsAlive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        SiddosA3MMeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; }

        private SiddosA3MQuickReportBuilder _reportBuilder;
        
        private SiddosA3MParser _parser;
        private FirmWaveQualifier _firmwareQualifier;

        
        public SiddosA3MSensor(IBluetoothAdapter adapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                BluetoothAdapter.SendData,
                DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
                DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            );
            _parser = new SiddosA3MParser(_firmwareQualifier, true);
            _reportBuilder = new SiddosA3MQuickReportBuilder();


            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;
            _parser.ExportMemoryFragment += MemoryRecieveHandler;

            //BluetoothAdapter.ConnectSucceed += ConnectHandler;
            //BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            // Получение планировщика UI для потока, который создал форму:
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        }

    private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            
            _firmwareQualifier = new FirmWaveQualifier(
                BluetoothAdapter.SendData,
                DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
                DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            );
            _parser = new SiddosA3MParser(_firmwareQualifier, true);
            _parser.MessageReceived += ReceiveHandler;
            
            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("СиддосА3М успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(string commandName, string dataValue)
        {
            if(!_activated)
            {
                ClearStatus();
                return;
            }

            switch (commandName) // TODO: replace to enum 
            {
                case "DeviceStatus":
                    /*/ Для замера /*/
                    if (_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = DynamographStatusAdapter.StringStatusToEnum(dataValue);
                    }
                    SensorData.Status = DynamographStatusAdapter.StringStatusToReport(dataValue);
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

        public async Task<bool> UpdateFirmware()
        {
            byte[] resp= { };
            byte[] fw_address = new byte[4];
            byte[] fw_size = new byte[2];



            resp = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"]); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 4).CopyTo(fw_address);

            resp = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]); ;
            if (0 == resp.Length)
                return false;
            resp.AsSpan().Slice(12, 2).CopyTo(fw_size);

            var req = new MessageCreator().CreateReadMessage(fw_address, fw_size);
            resp = await BluetoothAdapter.Exchange(req); ;
            if (0 == resp.Length)
                return false;

            var pp = new Ddim2.Ddim2Parser();
            string cmd;
            string dataValue;
            cmd = pp.DefineCommand(resp);
            dataValue = pp.ConvertToStringPayload(resp);
            if(null == dataValue || 0==dataValue.Length)
                return false;
            SensorData.Firmware = dataValue;
            return true;
        }


        public async Task QuickReport()
        {
            byte[] bat_rs = { };
            Ddim2.Ddim2Parser pp=new Ddim2.Ddim2Parser();
            string cmd;
            string dataValue;

            bat_rs = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["BatteryVoltage"]); ;
            if(0!=bat_rs.Length)
            {
                cmd = pp.DefineCommand(bat_rs);
                dataValue = pp.ConvertToStringPayload(bat_rs);
                SensorData.Battery = dataValue;
                //_reportBuilder.BatteryVoltage = dataValue;
            }
            bat_rs = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["Тemperature"]); ;
            if (0 != bat_rs.Length)
            {
                cmd = pp.DefineCommand(bat_rs);
                dataValue = pp.ConvertToStringPayload(bat_rs);
                SensorData.Temperature = dataValue;
                //_reportBuilder.Temperature = dataValue;
            }
            bat_rs = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["LoadChanel"]); ;
            if (0 != bat_rs.Length)
            {
                cmd = pp.DefineCommand(bat_rs);
                dataValue = pp.ConvertToStringPayload(bat_rs);
                _reportBuilder.Load = dataValue;
            }
            bat_rs = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["AccelerationChanel"]); ;
            if (0 != bat_rs.Length)
            {
                cmd = pp.DefineCommand(bat_rs);
                dataValue = pp.ConvertToStringPayload(bat_rs);
                _reportBuilder.Acceleration = dataValue;
            }
            SensorData.Status = _reportBuilder.GetReport();
            /*
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["AccelerationChanel"]);
            */
        }

        public async Task<bool> KillosParametersQuery()
        {
            byte[] resp = { };
            Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();
            string cmd;
            string dataValue;

            resp = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["SensorLoadRKP"]); ;
            if (0 == resp.Length)
                return false;
            cmd = pp.DefineCommand(resp);
            dataValue = pp.ConvertToStringPayload(resp);
            _reportBuilder.SensitivityLoad = dataValue;

            resp = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["SensorLoadNKP"]); ;
            if (0 == resp.Length)
                return false;
            cmd = pp.DefineCommand(resp);
            dataValue = pp.ConvertToStringPayload(resp);
            _reportBuilder.ZeroOffsetLoad = dataValue;
            return true;

            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadRKP"]);
            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadNKP"]);
        }

        public async Task<bool> CheckStatus()
        {
            byte[] resp = { };
            Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();
            string cmd;
            string dataValue;

            resp = await BluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]); ;
            if (0 == resp.Length)
                return false;

            cmd = pp.DefineCommand(resp);
            dataValue = pp.ConvertToStringPayload(resp);
            _measurementManager.MeasurementStatus = DynamographStatusAdapter.StringStatusToEnum(dataValue);
            SensorData.Status = DynamographStatusAdapter.StringStatusToReport(dataValue);

            return true;
            //await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]);
            //return true;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await Task.Delay(1000);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (IsAlive)
                    {
                        if (!IsMeasurement)
                        {
                            await QuickReport();
                        }
                        else
                        {
                            await Task.Delay(1000, cancellationToken);
                            //await CheckStatus();
                        }
                        await Task.Delay(1000, cancellationToken);
                    }
                    else
                    {
                        SensorData.Status = "starting BT...";
                        bool connected = await BluetoothAdapter.Connect();
                        if(!connected)
                            await Task.Delay(2000, cancellationToken);
                        else
                        {
                            if (await UpdateFirmware() && await KillosParametersQuery())
                            {
                                IsAlive = true;
                                SensorData.Status = Resource.ConnectedStatus;
                            }
                                
                        }
                            
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                //Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                //Thread.Sleep(5000);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            //_parser.MessageReceived -= ReceiveHandler;
            SiddosA3MMeasurementStartParameters specificMeasurementParameters =
                (SiddosA3MMeasurementStartParameters)measurementParameters;
            _measurementManager = new SiddosA3MMeasurementManager(BluetoothAdapter, SensorData,
                specificMeasurementParameters);
            //_parser.ExportMemoryFragment += _measurementManager.MemoryRecieveHandler;
            var report = await _measurementManager.RunMeasurement();
            if(null!= report)
            {
                SensorService.Instance.MeasurementHandler(report);
                SensorData.Status = "measure [100%] - end";
            }
            else 
            {
                SensorData.Status = "measure [---%] - ERROR";
            }
            await Task.Delay(2000);
            IsMeasurement = false;
        }

        /*/ Для замера /*/
        private void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            _measurementManager.MeasurementRecieveHandler(commandName, data);
        }

        private void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if(_measurementManager != null)
            {
                _measurementManager.MemoryRecieveHandler(address, data);
                SensorData.Status = DynamographStatusAdapter.CreateProgressStatus(_measurementManager.Progress);
            }
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }
    }
}
