using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class MessageCreator
    {
        /// <summary>
        /// Калькулятор контрольной суммы
        /// </summary>
        private CrcModbusCalculator _crcModbusCalculator =
            new CrcModbusCalculator();

        /// <summary>
        /// Создать сообщение чтения
        /// </summary>
        /// <param name="address"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public byte[] CreateReadMessage(byte[] address, byte[] dataSize)
        {
            var bufList = new List<byte>() { 0x01, 0x01 };
            bufList.AddRange(address);
            for (int i = 0; i < 4 - address.Length; i++)
            {
                bufList.Add(0x00);
            }

            bufList.AddRange(dataSize);

            var crc = _crcModbusCalculator.ModbusCrc(bufList.ToArray());

            List<byte> result = new List<byte>() { 0x0D, 0x0A };
            result.AddRange(bufList);
            result.AddRange(crc);

            return result.ToArray();
        }

        /// <summary>
        /// Создать сообщение записи
        /// </summary>
        /// <param name="address"></param>
        /// <param name="dataSize"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] CreateWriteMessage(byte[] address, byte[] dataSize, byte[] data)
        {
            var addressAndSizeList = new List<byte>() { 0x01, 0x02 };
            addressAndSizeList.AddRange(address);
            for (int i = 0; i < 4 - address.Length; i++)
            {
                addressAndSizeList.Add(0x00);
            }

            addressAndSizeList.AddRange(dataSize);

            var crcAS = _crcModbusCalculator.ModbusCrc(addressAndSizeList.ToArray());
            var crcData = _crcModbusCalculator.ModbusCrc(data);

            List<byte> result = new List<byte>() { 0x0D, 0x0A };
            result.AddRange(addressAndSizeList);
            result.AddRange(crcAS);
            result.AddRange(data);
            result.AddRange(crcData);

            return result.ToArray();
        }

        /// <summary>
        /// Создать сообщение записи
        /// </summary>
        /// <param name="deviceNameAddressAndSize"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] CreateWriteMessage(byte[] deviceNameAddressAndSize, byte[] data)
        {
            var addressAndSizeList = new List<byte>() { 0x01, 0x02 };
            addressAndSizeList.AddRange(deviceNameAddressAndSize);

            var crcAS = _crcModbusCalculator.ModbusCrc(addressAndSizeList.ToArray());
            var crcData = _crcModbusCalculator.ModbusCrc(data);

            List<byte> result = new List<byte>() { 0x0D, 0x0A };
            result.AddRange(addressAndSizeList);
            result.AddRange(crcAS);
            result.AddRange(data);
            result.AddRange(crcData);

            return result.ToArray();
        }
    }
}
