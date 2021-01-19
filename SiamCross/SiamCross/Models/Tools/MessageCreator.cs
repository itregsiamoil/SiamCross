using System.Collections.Generic;

namespace SiamCross.Models.Tools
{
    public class MessageCreator
    {
        /// <summary>
        /// Создать сообщение чтения
        /// </summary>
        /// <param name="address"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public byte[] CreateReadMessage(byte[] address, byte[] dataSize)
        {
            List<byte> bufList = new List<byte>() { 0x01, 0x01 };
            bufList.AddRange(address);
            for (int i = 0; i < 4 - address.Length; i++)
            {
                bufList.Add(0x00);
            }

            bufList.AddRange(dataSize);

            byte[] crc = CrcModbusCalculator.ModbusCrc(bufList.ToArray());

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
            List<byte> addressAndSizeList = new List<byte>() { 0x01, 0x02 };
            addressAndSizeList.AddRange(address);
            for (int i = 0; i < 4 - address.Length; i++)
            {
                addressAndSizeList.Add(0x00);
            }

            addressAndSizeList.AddRange(dataSize);

            byte[] crcAS = CrcModbusCalculator.ModbusCrc(addressAndSizeList.ToArray());
            byte[] crcData = CrcModbusCalculator.ModbusCrc(data);

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
            List<byte> addressAndSizeList = new List<byte>() { 0x01, 0x02 };
            addressAndSizeList.AddRange(deviceNameAddressAndSize);

            byte[] crcAS = CrcModbusCalculator.ModbusCrc(addressAndSizeList.ToArray());
            byte[] crcData = CrcModbusCalculator.ModbusCrc(data);

            List<byte> result = new List<byte>() { 0x0D, 0x0A };
            result.AddRange(addressAndSizeList);
            result.AddRange(crcAS);
            result.AddRange(data);
            result.AddRange(crcData);

            return result.ToArray();
        }
    }
}
