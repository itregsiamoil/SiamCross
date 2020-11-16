using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Parser
    {
        private static readonly NLog.Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        /// <summary>
        /// Буффер сообщений
        /// </summary>
        private ByteBuffer _byteBuffer;

        // Делегат обработки данных из сообшений
        public delegate void DataHandler(string dataName, string dataValue);

        /// <summary>
        /// Событие принятия сообщения
        /// </summary>
        public event DataHandler MessageReceived;

        /// <summary>
        /// Получены данные графика
        /// </summary>
        public Action<string, byte[]> ByteMessageReceived { get; set; }

        ///// <summary>
        ///// Определитель имени устройства
        ///// </summary>
        //private DeviceNameQualifier _deviceNameQualifier;

        /// <summary>
        /// Определитель версии прошивки
        /// </summary>
        private FirmWaveQualifier _deviceFirmWaveQualifier;

        public Ddin2Parser(FirmWaveQualifier deviceFirmWaveQualifier,
            bool isResponseCheck)
        {
            _deviceFirmWaveQualifier = deviceFirmWaveQualifier;
            _byteBuffer = new ByteBuffer(isResponseCheck);
        }

        /// <summary>
        /// Обработать входящие байты
        /// </summary>
        /// <param name="inputBytes"></param>
        public void ByteProcess(byte[] inputBytes)
        {
            try
            {
                var message = _byteBuffer.AddBytes(inputBytes);
                if (message.Length == 0)
                {
                    return;
                }

                var commandName = DefineCommand(message);
                var commandData = ConvertToStringPayload(message);

                switch (commandName)
                {
                    //case "DeviceNameAddress":
                    //    _commandDictionary.Add(
                    //        new byte[] { message[12], message[13], message[14], message[15] },
                    //        "DeviceName");
                    //    _deviceNameQualifier.DeviceNameAddress = GetPayload(message);
                    //    break;
                    //case "DeviceNameSize":
                    //    _deviceNameQualifier.DeviceNameSize = GetPayload(message);
                    //    break;
                    case "ProgrammVersionAddress":
                        _commandDictionary.Add(
                            new byte[] { message[12], message[13], message[14], message[15] },
                            "DeviceProgrammVersion");
                        _deviceFirmWaveQualifier.DeviceNameAddress = GetPayload(message);
                        break;
                    case "ProgrammVersionSize":
                        _deviceFirmWaveQualifier.DeviceNameSize = GetPayload(message);
                        break;
                    case "Program":
                        var adr = BitConverter.ToString(new[] { message[5], message[4] });
                        MessageReceived?.Invoke(commandName, adr);
                        break;
                    case "ExportDynGraph":
                        ExportByteData(commandName, message);
                        break;
                    case "ExportAccelerationGraph":
                        ExportByteData(commandName, message);
                        break;
                    case "ReadMeasurementReport":
                        ExportByteData(commandName, message);
                        Console.WriteLine("Read Measurement Report");
                        break;
                    case "ReadMeasurementErrorCode":
                        ExportByteData(commandName, message);
                        break;
                    case "DgmPart1":
                    case "DgmPart2":
                        ExportByteData(commandName, message);
                        break;
                }
                //Если команда чтения
                if (message[3] == 0x01)
                {
                    MessageReceived?.Invoke(commandName, commandData);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ByteProcess" + ex.StackTrace + "\n");
                _logger.Error(ex, "ByteBuffer is recreate, throw force skip!" + "\n");
                _byteBuffer = new ByteBuffer(_byteBuffer.IsResponseCheck);
               // throw;
            }
        }

        private void ExportByteData(string commandName, byte[] message)
        {
            var points = GetPayload(message);
            ByteMessageReceived?.Invoke(commandName, points);
        }

        /// <summary>
        /// Получить полезную информацию из сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static private byte[] GetPayload(byte[] message)
        {
            //Console.WriteLine("GetPayload READ:" + BitConverter.ToString(message));
            int payloadSize = message[8] + message[9] * 16;                // 9ый байт указывает на размер данных
            var payloadBytes = new byte[payloadSize];
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
        static public string ConvertToStringPayload(byte[] message)
        {
            var payloadBytes = GetPayload(message);

            string result = "Данные потеряны";
            var dataType = DefineDataType(message);
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
                    foreach (var character in payloadBytes.Reverse())
                    {
                        result += BitConverter.ToString(new byte[] { character });
                    }

                    result += "XX";
                    break;
                case "report":
                    string resultString = "";
                    foreach (var oneByte in payloadBytes.Reverse())
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
        static public string DefineDeviceType(string deviceCode)
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
        static public string DefineDataType(byte[] message)
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
        static public string DefineCommand(byte[] message)
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
            var commandBytes = new byte[]
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
                foreach (var key in _commandDictionary.Keys)
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
        static private Dictionary<byte[], string> _commandDictionary = new Dictionary<byte[], string>()
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
