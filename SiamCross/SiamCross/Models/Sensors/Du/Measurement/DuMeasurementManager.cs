using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementManager
    {
        private IBluetoothAdapter _bluetoothAdapter;
        private DuMeasurementStartParameters _measurementParameters;
        private CommandGenerator _commandGenerator;

        public SensorData SensorData { get; private set; }

        public DuMeasurementStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentEchogram;

        public DuMeasurementManager(IBluetoothAdapter bluetoothAdapter, SensorData sensorData,
            DuMeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            _measurementParameters = measurementParameters;

            _commandGenerator = new CommandGenerator();
            _currentEchogram = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            Debug.WriteLine("SENDING PARAMETERS");
            await SendParameters();
            await Start();
            return null;
        }

        private async Task Start()
        {
            
        }

        private async Task SendParameters()
        {
            string firstByte = _measurementParameters.Inlet ? "00000001" : "00000000";
            string secondByte = _measurementParameters.Amplification ? "00000001" : "00000000";

            byte[] data = new byte[] 
            { 
                Convert.ToByte(firstByte, 2), 
                Convert.ToByte(secondByte, 2) 
            };

            byte[] command = _commandGenerator.GenerateWriteCommand(
                DuCommands.FullCommandDictionary[DuCommandsEnum.Revbit], data);

            await _bluetoothAdapter.SendData(command);
        }
    }
}
