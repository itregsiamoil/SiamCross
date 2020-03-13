using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2
{
    public class CommandGenerator
    {
        /// <summary>
        /// Калькулятор контрольной суммы
        /// </summary>
        private CrcModbusCalculator _crcCalculator = new CrcModbusCalculator();

        /// <summary>
        /// Преобразовать команду чтения в команду записи
        /// </summary>
        /// <param name="readCommand"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GenerateWriteCommand(byte[] readCommand, byte[] data)
        {
            var bufList = new List<byte>();
            bufList.AddRange(readCommand);
            bufList.RemoveAt(0);
            bufList.RemoveAt(0);
            bufList.RemoveAt(bufList.Count - 1);
            bufList.RemoveAt(bufList.Count - 1);
            bufList[1] = 0x02;

            var crc = _crcCalculator.ModbusCrc(bufList.ToArray());

            var result = new List<byte>() { 0x0D, 0x0A };
            result.AddRange(bufList);
            result.AddRange(crc);

            var dataList = new List<byte>();
            dataList.AddRange(data);
            for (int i = 0; i < result[8] - data.Count(); i++)
            {
                dataList.Add(0x00);
            }

            result.AddRange(dataList);
            result.AddRange(_crcCalculator.ModbusCrc(dataList.ToArray()));

            return result.ToArray();
        }

        public byte[] SetDeviceNumber(int value)
        {
            var number = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["DeviceNumber"];

            return GenerateWriteCommand(command, number);
        }

        public byte[] SetRod(int value)
        {
            var rod = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["Rod"];
            return GenerateWriteCommand(command, rod);
        }

        public byte[] SetDynPeriod(int value)
        {
            var dyn = BitConverter.GetBytes(Convert.ToUInt32(value));
            byte[] command = DynamographCommands.FullCommandDictionary["DynPeriod"];

            return GenerateWriteCommand(command, dyn);
        }

        public byte[] SetApertNumber(int value)
        {
            var apert = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["ApertNumber"];

            return GenerateWriteCommand(command, apert);
        }
        public byte[] SetImtravel(int value)
        {
            var imravel = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["Imtravel"];

            return GenerateWriteCommand(command, imravel);
        }

        public byte[] SetModelPump(int value)
        {
            var pump = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["ModelPump"];

            return GenerateWriteCommand(command, pump);
        }
        public byte[] SensorLoadNKP(float value)
        {
            var nkp = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SensorLoadNKP"];

            return GenerateWriteCommand(command, nkp);
        }

        public byte[] SensorLoadRKP(float value)
        {
            var rkp = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SensorLoadRKP"];

            return GenerateWriteCommand(command, rkp);
        }

        public byte[] SensorAcceleration0G(float value)
        {
            var zeroG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SensorAcceleration0G"];

            return GenerateWriteCommand(command, zeroG);
        }

        public byte[] SensorAcceleration1G(float value)
        {
            var oneG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SensorAcceleration1G"];

            return GenerateWriteCommand(command, oneG);
        }

        public byte[] SwitchingInterval(int value)
        {
            var interval = BitConverter.GetBytes(Convert.ToUInt32(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SwitchingInterval"];

            return GenerateWriteCommand(command, interval);
        }

        public byte[] SensorAccelerationMinus1G(float value)
        {
            var minusOneG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SensorAccelerationMinus1G"];

            return GenerateWriteCommand(command, minusOneG);
        }

        public byte[] ZeroOffsetTemperature(float value)
        {
            var zeroOffsetTemperature = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["ZeroOffsetTemperature"];

            return GenerateWriteCommand(command, zeroOffsetTemperature);
        }

        public byte[] SlopeFactorTemperature(float value)
        {
            var slope = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DynamographCommands.FullCommandDictionary["SlopeFactorTemperature"];

            return GenerateWriteCommand(command, slope);
        }

        public byte[] OffInterval(int value)
        {
            var interval = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["TimeOff"];

            return GenerateWriteCommand(command, interval);
        }

        public byte[] EnableOff(int value)
        {
            var flag = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DynamographCommands.FullCommandDictionary["EnableTimeOff"];

            return GenerateWriteCommand(command, flag);
        }
    }
}