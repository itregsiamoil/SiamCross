using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.Ddin2
{
    public class Ddin2Parser
    {
        /// <summary>
        /// Буффер сообщений
        /// </summary>
        private readonly ByteBuffer _byteBuffer;

        // Делегат обработки данных из сообшений
        public delegate void DataHandler(string dataName, string dataValue);

        ///// <summary>
        ///// Определитель имени устройства
        ///// </summary>
        //private DeviceNameQualifier _deviceNameQualifier;

        /// <summary>
        /// Определитель версии прошивки
        /// </summary>
        public Ddin2Parser()
        {
            _byteBuffer = new ByteBuffer(false);
        }

        /// <summary>
        /// Получить полезную информацию из сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] GetPayload(byte[] message)
        {
            //Console.WriteLine("GetPayload READ:" + BitConverter.ToString(message));
            int payloadSize = message[8] + message[9] * 16;                // 9ый байт указывает на размер данных
            byte[] payloadBytes = new byte[payloadSize];
            if (message.Length > 12)
            {
                for (int i = 0; i < payloadSize; i++)
                {
                    payloadBytes[i] =
                        message[12 + i]; // с 13 байта начинается полезная нагрузка, которая имеет размер payloadSize
                }
            }

            return payloadBytes;
        }

        /// <summary>
        /// Преобразовать полезные данные в строку
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string ConvertToStringPayload(byte[] message)
        {
            byte[] payloadBytes = GetPayload(message);

            string result = "Данные потеряны";
            string dataType = DefineDataType(message);
            switch (dataType)
            {
                case "int32":
                    result = BitConverter.ToInt32(payloadBytes, 0).ToString();
                    break;
                case "float":
                    result = BitConverter.ToSingle(payloadBytes, 0).ToString();
                    break;
                case "int16":
                    result = BitConverter.ToInt16(payloadBytes, 0).ToString();
                    break;
                case "Device":
                    result = DefineDeviceType(
                        BitConverter.ToString(payloadBytes, 0).ToString());
                    break;
                case "S16":
                    float value = BitConverter.ToInt16(payloadBytes, 0);
                    result = (value / 10).ToString();
                    break;
                case "string":
                    if (payloadBytes.Length > 20)
                    {
                        result = Encoding.UTF8.GetString(payloadBytes);
                    }
                    else
                    {
                        result = Encoding.GetEncoding(1251).GetString(payloadBytes);
                    }
                    //result = Encoding.UTF8.GetString(payloadBytes);
                    //result = Encoding.GetEncoding
                    break;
                case "Program":
                    result = ":" + BitConverter.ToString(new byte[] { message[8] })
                                 + BitConverter.ToString(new byte[] { message[5] })
                                 + BitConverter.ToString(new byte[] { message[4] }) + "00";
                    foreach (byte character in payloadBytes.Reverse())
                    {
                        result += BitConverter.ToString(new byte[] { character });
                    }

                    result += "XX";
                    break;
                case "report":
                    string resultString = "";
                    foreach (byte oneByte in payloadBytes.Reverse())
                    {
                        resultString = resultString + oneByte + " ";
                    }
                    result = resultString;
                    break;
                case "graph":

                    break;
            }

            return result;
        }

        /// <summary>
        /// Определить ТИП устройства
        /// </summary>
        /// <param name="deviceCode"></param>
        /// <returns></returns>
        public static string DefineDeviceType(string deviceCode)
        {
            switch (deviceCode)
            {
                case "01-14":
                    return "ДДИМ-2";
                case "02-13":
                    return "ДДИН-2";
                default:
                    return "Неизвестное устройство";
            }
        }

        /// <summary>
        /// Определить тип данных
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string DefineDataType(byte[] message)
        {
            if (message[7] == 0xFF)
            {
                return "Program";
            }

            if (message[7] == 0x81) //DynGraph
            {
                return "dynGraph";
            }

            if (message[7] == 0x83) //Accelerataion Graph
            {
                return "accelerationGraph";
            }

            if (message[5] == 0x00)             // Общие регистры
            {
                if (message[4] == 0x00)
                {
                    if (message[6] == 0x00 && message[7] == 0x80)
                    {
                        return "report";
                    }
                    return "Device";
                }
                else if (message[4] == 0x08)
                {
                    return "int16";
                }
                else
                {
                    return "int32";
                }

            }
            else if (message[5] == 0x10)        // Информационные регистры
            {
                if (message[4] == 0x04)
                {
                    return "int16";
                }
                else
                {
                    return "int32";
                }
            }
            else if (message[5] == 0x80)        // Параметры измерения
            {
                if (message[4] == 0x02) //DynPeriod U32
                {
                    return "int32";
                }
                return "int16";
            }
            else if (message[5] == 0x81)        // Энергозависимые параметры
            {
                if (message[4] != 0x10 &&
                    message[4] != 0x20 &&
                    message[4] != 0x22)
                {
                    return "float";
                }
                else if (message[4] == 0x10)
                {
                    return "int32";
                }
                else
                {
                    return "int16";
                }
            }
            else if (message[5] == 0x84)        // Текущие параметры
            {
                if (message[4] == 0x00 || message[4] == 0x02)
                {
                    return "S16";
                }
                else
                {
                    return "float";
                }
            }
            else if (message[5] == 0x88 && message[4] == 0x02) // Статус измерения 
            {
                return "int16";
            }
            else if (message[5] == 0x88 && message[4] == 0x04) // Код ошибки измерения
            {
                return "int16";
            }

            else return "string";
        }

        /// <summary>
        /// Определить комманду
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string DefineCommand(byte[] message)
        {
            if (message[3] == 0x02 && message[7] == 0xFF)
            {
                return "Program";
            }

            if (message[3] == 0x01 && message[7] == 0xFF)
            {
                return "ProgramReading";
            }

            string commandName = "";
            byte[] commandBytes = new byte[]
            {
                message[4],
                message[5],
                message[6],
                message[7]
            }; // байты указывающие тип комманды
            if (commandBytes[3] == 0x81) //Если чтение графика динамограммы
            {
                //var bytes = new byte[] { message[8], message[9] };
                //var messageLength = BitConverter.ToInt16(bytes, 0);
                //if (messageLength > 20)
                //{
                //    if (messageLength == 1000)
                //    {
                //        commandName = "DgmPart1";
                //    }
                //    else
                //    {
                //        commandName = "DgmPart2";
                //    }
                //}
                //else
                //{
                //    commandName = "ExportDynGraph";
                //}
                commandName = "DgmPart1";
            }
            else if (commandBytes[3] == 0x83) //Если чтение графика ускорения
            {
                commandName = "ExportAccelerationGraph";
            }
            else
            {
                foreach (byte[] key in _commandDictionary.Keys)
                {
                    if (key[0] == commandBytes[0] &&
                       key[1] == commandBytes[1] &&
                       key[2] == commandBytes[2] &&
                       key[3] == commandBytes[3])
                    {
                        commandName = _commandDictionary[key];
                    }
                }
            }

            return commandName;
        }

        /// <summary>
        /// Словарь соответствия адрессов командам
        /// </summary>
        private static readonly Dictionary<byte[], string> _commandDictionary = new Dictionary<byte[], string>()
        {
            { new byte[]{ 0x00, 0x00, 0x00, 0x00}, "DeviceType" },
            { new byte[]{ 0x02, 0x00, 0x00, 0x00}, "MemoryModelVersion" },
            { new byte[]{ 0x04, 0x00, 0x00, 0x00}, "DeviceNameAddress" },
            { new byte[]{ 0x08, 0x00, 0x00, 0x00}, "DeviceNameSize" },
            { new byte[]{ 0x0A, 0x00, 0x00, 0x00}, "DeviceNumber" },
            { new byte[]{ 0x00, 0x10, 0x00, 0x00}, "ProgrammVersionAddress" },
            { new byte[]{ 0x04, 0x10, 0x00, 0x00}, "ProgrammVersionSize" },
            { new byte[]{ 0x00, 0x80, 0x00, 0x00}, "Rod" },
            { new byte[]{ 0x02, 0x80, 0x00, 0x00}, "DynPeriod" },
            { new byte[]{ 0x06, 0x80, 0x00, 0x00}, "ApertNumber" },
            { new byte[]{ 0x08, 0x80, 0x00, 0x00}, "Imtravel" },
            { new byte[]{ 0x0A, 0x80, 0x00, 0x00}, "ModelPump" },
            { new byte[]{ 0x00, 0x81, 0x00, 0x00}, "SensorLoadNKP" },
            { new byte[]{ 0x04, 0x81, 0x00, 0x00}, "SensorLoadRKP" },
            { new byte[]{ 0x08, 0x81, 0x00, 0x00}, "SensorAcceleration0G" },
            { new byte[]{ 0x0C, 0x81, 0x00, 0x00}, "SensorAcceleration1G" },
            { new byte[]{ 0x10, 0x81, 0x00, 0x00}, "SwitchingInterval" },
            { new byte[]{ 0x14, 0x81, 0x00, 0x00}, "ZeroOffset" },
            { new byte[]{ 0x18, 0x81, 0x00, 0x00}, "SlopeFactor" },
            { new byte[]{ 0x1C, 0x81, 0x00, 0x00}, "SensorAccelerationMinus1G" },
            { new byte[]{ 0x20, 0x81, 0x00, 0x00}, "EnableTimeOff" },
            { new byte[]{ 0x22, 0x81, 0x00, 0x00}, "TimeOff" },
            { new byte[]{ 0x00, 0x84, 0x00, 0x00}, "BatteryVoltage" },
            { new byte[]{ 0x02, 0x84, 0x00, 0x00}, "Тemperature" },
            { new byte[]{ 0x04, 0x84, 0x00, 0x00}, "LoadChanel" },
            { new byte[]{ 0x08, 0x84, 0x00, 0x00}, "AccelerationChanel" },
            { new byte[]{ 0x02, 0x88, 0x00, 0x00}, "DeviceStatus" },
            { new byte[]{ 0x00, 0x00, 0x00, 0x80}, "ReadMeasurementReport" },
            { new byte[]{ 0x04, 0x88, 0x00, 0x00}, "ReadMeasurementErrorCode" },
            { new byte[]{ 0x00, 0x00, 0x00, 0x81}, "ExportDynGraph" },
            { new byte[]{ 0x00, 0x00, 0x00, 0x83}, "ExportAccelerationGraph" }

        };
    }
}
