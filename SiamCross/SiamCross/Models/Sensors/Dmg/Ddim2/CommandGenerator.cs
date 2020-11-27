using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.Ddim2
{
    public class CommandGenerator
    {
        /// <summary>
        /// Калькулятор контрольной суммы
        /// </summary>

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

            var crc = CrcModbusCalculator.ModbusCrc(bufList.ToArray());

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
            result.AddRange(CrcModbusCalculator.ModbusCrc(dataList.ToArray()));

            return result.ToArray();
        }

        public byte[] SetDeviceNumber(int value)
        {
            var number = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("DeviceNumber");

            return GenerateWriteCommand(command, number);
        }

        public byte[] SetRod(int value)
        {
            var rod = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("Rod");
            return GenerateWriteCommand(command, rod);
        }

        public byte[] SetDynPeriod(int value)
        {
            var dyn = BitConverter.GetBytes(Convert.ToUInt32(value));
            byte[] command = DmgCmd.Get("DynPeriod");

            return GenerateWriteCommand(command, dyn);
        }

        public byte[] SetApertNumber(int value)
        {
            var apert = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("ApertNumber");

            return GenerateWriteCommand(command, apert);
        }
        public byte[] SetImtravel(int value)
        {
            var imravel = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("Imtravel");

            return GenerateWriteCommand(command, imravel);
        }

        public byte[] SetModelPump(int value)
        {
            var pump = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("ModelPump");

            return GenerateWriteCommand(command, pump);
        }
        public byte[] SensorLoadNKP(float value)
        {
            var nkp = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SensorLoadNKP");

            return GenerateWriteCommand(command, nkp);
        }

        public byte[] SensorLoadRKP(float value)
        {
            var rkp = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SensorLoadRKP");

            return GenerateWriteCommand(command, rkp);
        }

        public byte[] SensorAcceleration0G(float value)
        {
            var zeroG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SensorAcceleration0G");

            return GenerateWriteCommand(command, zeroG);
        }

        public byte[] SensorAcceleration1G(float value)
        {
            var oneG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SensorAcceleration1G");

            return GenerateWriteCommand(command, oneG);
        }

        public byte[] SwitchingInterval(int value)
        {
            var interval = BitConverter.GetBytes(Convert.ToUInt32(value));
            byte[] command = DmgCmd.Get("SwitchingInterval");

            return GenerateWriteCommand(command, interval);
        }

        public byte[] SensorAccelerationMinus1G(float value)
        {
            var minusOneG = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SensorAccelerationMinus1G");

            return GenerateWriteCommand(command, minusOneG);
        }

        public byte[] ZeroOffsetTemperature(float value)
        {
            var zeroOffsetTemperature = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("ZeroOffsetTemperature");

            return GenerateWriteCommand(command, zeroOffsetTemperature);
        }

        public byte[] SlopeFactorTemperature(float value)
        {
            var slope = BitConverter.GetBytes(Convert.ToSingle(value));
            byte[] command = DmgCmd.Get("SlopeFactorTemperature");

            return GenerateWriteCommand(command, slope);
        }

        public byte[] OffInterval(int value)
        {
            var interval = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("TimeOff");

            return GenerateWriteCommand(command, interval);
        }

        public byte[] EnableOff(int value)
        {
            var flag = BitConverter.GetBytes(Convert.ToUInt16(value));
            byte[] command = DmgCmd.Get("EnableTimeOff");

            return GenerateWriteCommand(command, flag);
        }
    }
}