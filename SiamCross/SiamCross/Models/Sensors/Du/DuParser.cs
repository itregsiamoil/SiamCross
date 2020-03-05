using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Sensors.Du
{
    public class DuParser
    {
        private static readonly NLog.Logger _logger = AppContainer.Container
           .Resolve<ILogManager>().GetLog();
        /// <summary>
        /// Буффер сообщений
        /// </summary>
        private ByteBuffer _byteBuffer = new ByteBuffer();

        // Делегат обработки данных из сообшений
        public delegate void DataHandler(DuCommandsEnum dataName, string dataValue);

        /// <summary>
        /// Событие принятия сообщения
        /// </summary>
        public event DataHandler MessageReceived;

        /// <summary>
        /// Получены данные графика
        /// </summary>
        public Action<DuCommandsEnum, byte[]> ByteMessageReceived { get; set; }

        public static Logger Logger => _logger;

        ///// <summary>
        ///// Определитель имени устройства
        ///// </summary>
        //private DeviceNameQualifier _deviceNameQualifier;

        /// <summary>
        /// Определитель версии прошивки
        /// </summary>
        private FirmWaveQualifier _deviceFirmWaveQualifier;

        public DuParser(FirmWaveQualifier deviceFirmWaveQualifier)
        {
            _deviceFirmWaveQualifier = deviceFirmWaveQualifier;
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
                    case DuCommandsEnum.ProgrammVersionAddress:
                        _commandDictionary.Add(
                            new byte[] { message[12], message[13], message[14], message[15] },
                            DuCommandsEnum.DeviceProgrammVersion);
                        _deviceFirmWaveQualifier.DeviceNameAddress = GetPayload(message);
                        break;
                    case DuCommandsEnum.ProgrammVersionSize:
                        _deviceFirmWaveQualifier.DeviceNameSize = GetPayload(message);
                        break;
                    case DuCommandsEnum.ResearchData:
                        ExportByteData(DuCommandsEnum.ResearchData, message);
                        break;
                    case DuCommandsEnum.ResearchTitle:
                        ExportByteData(DuCommandsEnum.ResearchTitle, message);
                        Console.WriteLine("Read Measurement Report");
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
                _logger.Error(ex, "ByteProcess " + ex.StackTrace);
                _logger.Error(ex, "ByteBuffer is recreate, throw force skip!");
                _byteBuffer = new ByteBuffer();
            }
        }

        private void ExportByteData(DuCommandsEnum commandName, byte[] message)
        {
            var points = GetPayload(message);
            ByteMessageReceived?.Invoke(commandName, points);
        }

        /// <summary>
        /// Получить полезную информацию из сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private byte[] GetPayload(byte[] message)
        {
            int payloadSize = message[8] + message[9] * 10;                // 9ый байт указывает на размер данных
            var payloadBytes = new byte[payloadSize];
            if (message.Length > 12)
            {
                for (int i = 0; i < payloadSize; i++)
                {
                    payloadBytes[i] =
                        message[12 + i]; // с 13 байта начинается полезная нагрузка, которая имеет размер payloadSize
                }
            }

            return payloadBytes; ///////////////////////////////////////////////////////////////////////////////////////////////////Баг выход за пределы массива
        }

        /// <summary>
        /// Преобразовать полезные данные в строку
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string ConvertToStringPayload(byte[] message)
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
        public string DefineDeviceType(string deviceCode)
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
        public string DefineDataType(byte[] message)
        {
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
            if (message[5] == 0x88 && message[4] == 0x00)
            {
                if (message[3] != 0x82)
                    Debug.WriteLine("///////////////////////////////////////////" +
                        "initOrStartMeasurement Answer//////////////////////////////////////");
                if (message[3] == 0x82)
                    Debug.WriteLine("Error///Error///Error///Error///Error///Error///" +
                        "initOrStartMeasurement AnswerError///Error///Error///Error///Error///");
                return "initOrStartMeasurement";
            }


            else return "string";
        }

        /// <summary>
        /// Определить комманду
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public DuCommandsEnum DefineCommand(byte[] message)
        {
            var commandBytes = new byte[]
            {
                message[4],
                message[5],
                message[6],
                message[7]
            }; 
            if (commandBytes[3] == 0x81) //Если чтение графика эхограммы
            {
                return DuCommandsEnum.ResearchData;
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
                        return _commandDictionary[key];
                    }
                }
            }

            return DuCommandsEnum.StubCommand;
        }

        /// <summary>
        /// Словарь соответствия адрессов командам
        /// </summary>
        private Dictionary<byte[], DuCommandsEnum> _commandDictionary = new Dictionary<byte[], DuCommandsEnum>()
        {
            { new byte[]{ 0x00, 0x00, 0x00, 0x00}, DuCommandsEnum.DeviceType },
            { new byte[]{ 0x02, 0x00, 0x00, 0x00}, DuCommandsEnum.MemoryModelVersion },
            { new byte[]{ 0x04, 0x00, 0x00, 0x00}, DuCommandsEnum.DeviceNameAddress },
            { new byte[]{ 0x08, 0x00, 0x00, 0x00}, DuCommandsEnum.DeviceNameSize },
            { new byte[]{ 0x0A, 0x00, 0x00, 0x00}, DuCommandsEnum.DeviceNumber },
            { new byte[]{ 0x00, 0x10, 0x00, 0x00}, DuCommandsEnum.ProgrammVersionAddress },
            { new byte[]{ 0x04, 0x10, 0x00, 0x00}, DuCommandsEnum.ProgrammVersionSize },
            { new byte[]{ 0x00, 0x80, 0x00, 0x00}, DuCommandsEnum.PressureSensitivity },
            { new byte[]{ 0x02, 0x80, 0x00, 0x00}, DuCommandsEnum.PiezoelectricSensitivity },
            { new byte[]{ 0x04, 0x80, 0x00, 0x00}, DuCommandsEnum.PressureZeroOffset },
            { new byte[]{ 0x06, 0x80, 0x00, 0x00}, DuCommandsEnum.PiezoelectricOffset },
            { new byte[]{ 0x08, 0x80, 0x00, 0x00}, DuCommandsEnum.Revbit },
            { new byte[]{ 0x00, 0x84, 0x00, 0x00}, DuCommandsEnum.Voltage },
            { new byte[]{ 0x02, 0x84, 0x00, 0x00}, DuCommandsEnum.Pressure },
            { new byte[]{ 0x00, 0x88, 0x00, 0x00}, DuCommandsEnum.StartMeasurement }, // по данному адрессу ответ на 5 комманд           
            { new byte[]{ 0x02, 0x88, 0x00, 0x00}, DuCommandsEnum.SensorState },
            { new byte[]{ 0x00, 0x00, 0x00, 0x80}, DuCommandsEnum.ResearchTitle },
            { new byte[]{ 0x00, 0x00, 0x00, 0x81}, DuCommandsEnum.ResearchData }
        };
    }
}
