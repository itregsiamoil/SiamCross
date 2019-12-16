﻿using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Ddim2.Measurement
{
    public class Ddim2MeasurementManager
    {
        private IBluetoothAdapter _bluetoothAdapter;
        private DeviceConfigCommandGenerator _configGenerator = new DeviceConfigCommandGenerator();

        private Ddim2MeasurementParameters _measurementParameters;
        private Ddim2SecondaryParameters _secondaryParameters;

        private Ddim2MeasurementReport _report;
        private byte[] _errorCode;
        private List<Ddim2MeasurementData> _measurements;

        public Ddim2MeasurementManager(IBluetoothAdapter bluetoothAdapter,
            Ddim2MeasurementParameters measurementParameters, Ddim2SecondaryParameters secondaryParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _measurementParameters = measurementParameters;
            _secondaryParameters = secondaryParameters;
        }

        public async Task RunMeasurement()
        {
            await SendParameters();
            await Start();
        }

        private async Task SendParameters()
        {
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.Rod));
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.DynPeriod));
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.ApertNumber));
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.Imtravel));
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.ModelPump));
        }

        private async Task Start()
        {
            await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["InitializeMeasurement"]);
            await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["StartMeasurement"]);
        }

        /*/ Copy from SiamBLE /*/
        public async Task<List<Ddim2MeasurementData>> DownloadMeasurement(Ddim2MeasurementStatusState measurementStatus)
        {
            bool gotError = false;

            if (measurementStatus == Ddim2MeasurementStatusState.Error)
            {
                gotError = true;
                await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["ReadMeasurementErrorCode"]);
            }

            var _currentReport = new List<byte>();
            var _currentDynGraph = new List<byte[]>();
            _currentDynGraph.Add(new byte[2] { 0, 0 });
            var _currentAccelerationGraph = new List<byte[]>();

            await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["ReadMeasurementReport"]);

            await Task.Delay(300);

            await GetDgm4kB();

            await Task.Delay(1000);

            var dynRawBytes = new List<byte>();
            foreach (var bytes in _currentDynGraph)
            {
                foreach (var b in bytes)
                {
                    dynRawBytes.Add(b);
                }
            }

            Ddim2MeasurementData measurement = gotError ? 
                new Ddim2MeasurementData(_report, dynRawBytes, DateTime.Now, null, _errorCode) : 
                new Ddim2MeasurementData(_report, dynRawBytes, DateTime.Now, null, null);                   ////////////////// ? 

            measurement.DynGraphPoints = DgmConverter.GetXYs(measurement.DynGraph.ToList(),
                    measurement.Report.Step, measurement.Report.WeightDiscr);

            _measurements.Add(measurement);
            //var row = new DataGridViewRow();
            //row.CreateCells(dataGridViewMeasurements);
            //row.Cells[0].Value = measurement.Date;
            //row.Cells[1].Value = measurement.ErrorCode;
            //dataGridViewMeasurements.Rows.Add(row);

            //Console.WriteLine($"Points count in DynGraph {_currentDynGraph.Count}");

            //ShowMeasurement(measurement);

            await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["ReadMeasurementReport"]);
            await _bluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["InitializeMeasurement"]);
            return _measurements;
        }

        private async Task GetDgm4kB()
        {
            byte[] length = BitConverter.GetBytes(20);
            var command = new List<byte>
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x81,
            };
            command.Add(length[0]);
            command.Add(length[1]);

            AddCrc();

            //Read first 500 bytes

            await _bluetoothAdapter.SendData(command.ToArray());
            //Thread.Sleep(900);

            RemoveCrc();

            //Read rest
            for (int i = 0; i < 198; i++)
            {
                short newAdress = (short)(BitConverter.ToInt16(new byte[] { command[4], command[5] }, 0) + 20);
                byte[] newAdressBytes = BitConverter.GetBytes(newAdress);
                command[4] = newAdressBytes[0];
                command[5] = newAdressBytes[1];

                AddCrc();
                await _bluetoothAdapter.SendData(command.ToArray());
                //Thread.Sleep(900);

                RemoveCrc();
            }

            void AddCrc()
            {
                var crcCalculator = new CrcModbusCalculator();
                byte[] crc = crcCalculator.ModbusCrc(command.GetRange(2, 8).ToArray());
                command.Add(crc[0]);
                command.Add(crc[1]);
            }

            void RemoveCrc()
            {
                command.RemoveAt(command.Count - 1);
                command.RemoveAt(command.Count - 1);
            }
        }
    }
}
