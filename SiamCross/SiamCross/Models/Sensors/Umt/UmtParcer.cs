using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SiamCross.Models.Sensors.Umt
{
    public class UmtParser
    {
        private static readonly NLog.Logger _logger = AppContainer.Container
           .Resolve<ILogManager>().GetLog();
        /// <summary>
        /// Буффер сообщений
        /// </summary>
        private ByteBuffer _byteBuffer;

        // Делегат обработки данных из сообшений
        public delegate void DataHandler(UmtCommandsEnum dataName, string dataValue);

        /// <summary>
        /// Событие принятия сообщения
        /// </summary>
        public event DataHandler MessageReceived;

        /// <summary>
        /// Получены данные графика
        /// </summary>
        public Action<UmtCommandsEnum, byte[]> ByteMessageReceived { get; set; }

        public static Logger Logger => _logger;

        /// <summary>
        /// Определитель версии прошивки
        /// </summary>
        private FirmWaveQualifier _deviceFirmWaveQualifier;

        public UmtParser(FirmWaveQualifier deviceFirmWaveQualifier,
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
                Debug.WriteLine("Umt: " + BitConverter.ToString(message) + "\n");

                var commandName = DefineCommand(message);
                var commandData = ConvertToStringPayload(message);
                switch (commandName)
                {
                    case UmtCommandsEnum.ProgrammVersionAddress:
                        _commandDictionary.Add(
                            new byte[] { message[12], message[13], message[14], message[15] },
                            UmtCommandsEnum.DeviceProgrammVersion);
                        _deviceFirmWaveQualifier.DeviceNameAddress = GetPayload(message);
                        break;
                    case UmtCommandsEnum.ProgrammVersionSize:
                        _deviceFirmWaveQualifier.DeviceNameSize = GetPayload(message);
                        break;
                    case UmtCommandsEnum.Revbit:
                        var payload = GetPayload(message);

                        Debug.WriteLine(BitConverter.ToString(payload));
                        Debug.WriteLine(AddZerosToBinary(Convert.ToString(payload[1], 2)));
                        Debug.WriteLine(AddZerosToBinary(Convert.ToString(payload[0], 2)));
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
                _logger.Error(ex, "ByteProcess " + ex.StackTrace + "\n");
                _logger.Error(ex, "ByteBuffer is recreate, throw force skip!" + "\n");
                Debug.WriteLine(BitConverter.ToString(_byteBuffer.Buffer.ToArray()));
                _byteBuffer = new ByteBuffer(_byteBuffer.IsResponseCheck);
            }
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

            return payloadBytes;
        }

        public string ConvertToStringPayload(byte[] message)
        {
            try
            {
                string result = "";
                var payloadBytes = GetPayload(message);
                var dataType = DefineDataType(message);

                switch (dataType)
                {
                    case DeviceRegistersTypes.Int32:
                        {
                            result = BitConverter.ToInt32(payloadBytes, 0).ToString();
                        }
                        break;
                    case DeviceRegistersTypes.Int16:
                        {
                            result = BitConverter.ToInt16(payloadBytes, 0).ToString();
                        }
                        break;
                    case DeviceRegistersTypes.Int8:
                        {
                            if (payloadBytes.Length == 1)
                            {
                                result = BitConverter.ToInt16(new byte[] { payloadBytes[0], 0x00 }, 0).ToString();
                            }
                        }
                        break;
                    case DeviceRegistersTypes.Float:
                        {
                            result = BitConverter.ToSingle(payloadBytes, 0).ToString();
                        }
                        break;
                    case DeviceRegistersTypes.String:
                        {
                            result = Encoding.UTF8.GetString(payloadBytes);
                        }
                        break;
                    case DeviceRegistersTypes.Time:
                        {
                            if (payloadBytes.Length == 3)
                            {
                                result = $"{payloadBytes[0]}:{payloadBytes[1]}:{payloadBytes[2]}";
                            }
                        }
                        break;
                    case DeviceRegistersTypes.DateTime:
                        {
                            if (payloadBytes.Length == 6)
                            {
                                result = $"{payloadBytes[0]}:{payloadBytes[1]}:{payloadBytes[2]}:" +
                                    $"{payloadBytes[3]}:{payloadBytes[4]}:{payloadBytes[5]}";
                            }
                        }
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Определить тип данных
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public DeviceRegistersTypes DefineDataType(byte[] message)
        {
            switch (message[7])
            {
                case 0x00:
                    {
                        switch (message[5])
                        {
                            case 0x00:
                                {
                                    switch (message[4])
                                    {
                                        case 0x04:
                                        case 0x0A:
                                            return DeviceRegistersTypes.Int32;
                                        case 0x1A:
                                            return DeviceRegistersTypes.Int8;
                                        default:
                                            return DeviceRegistersTypes.Int16;
                                    }
                                }
                            case 0x10:
                                {
                                    switch (message[4])
                                    {
                                        case 0x00:
                                            return DeviceRegistersTypes.Int32;
                                        case 0x04:
                                            return DeviceRegistersTypes.Int16;
                                    }
                                    break;
                                }
                            case 0x20:
                                {
                                    return DeviceRegistersTypes.InvalidBloks;
                                }
                            case 0x80:
                                {
                                    switch (message[4])
                                    {
                                        case 0x00:
                                            return DeviceRegistersTypes.Int8;
                                        case 0x01:
                                        case 0x06:
                                            return DeviceRegistersTypes.String;
                                        case 0x0C:
                                        case 0x0E:
                                        case 0x10:
                                        case 0x1A:
                                            return DeviceRegistersTypes.Int16;
                                        case 0x12:
                                            return DeviceRegistersTypes.Float;
                                        case 0x16:
                                            return DeviceRegistersTypes.Int32;

                                    }
                                    break;
                                }
                            case 0x84:
                                {
                                    switch (message[4])
                                    {
                                        case 0x00:
                                        case 0x04:
                                        case 0x08:
                                        case 0x0C:
                                        case 0x10:
                                        case 0x14:
                                            return DeviceRegistersTypes.Float;
                                        case 0x18:
                                        case 0x1A:
                                        case 0x1C:
                                        case 0x1E:
                                        case 0x20:
                                            return DeviceRegistersTypes.Int16;
                                        case 0x22:
                                            return DeviceRegistersTypes.Int32;
                                        case 0x26:
                                            return DeviceRegistersTypes.DateTime;
                                        case 0x2C:
                                            return DeviceRegistersTypes.Time;
                                    }
                                    break;
                                }
                            case 0x88:
                                {
                                    switch (message[4])
                                    {
                                        case 0x00:
                                            return DeviceRegistersTypes.Int8;
                                        case 0x02:
                                            return DeviceRegistersTypes.Int16;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 0x70:
                    {
                        return DeviceRegistersTypes.Page;
                    }
                case 0x80:
                    {
                        return DeviceRegistersTypes.ResearchHeader;
                    }
            }

            return DeviceRegistersTypes.String;
        }

        /// <summary>
        /// Определить комманду
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public UmtCommandsEnum DefineCommand(byte[] message)
        {
            var commandBytes = new byte[]
            {
                message[4],
                message[5],
                message[6],
                message[7]
            };

            if (commandBytes[3] == 0x70)
            {
                return UmtCommandsEnum.MeasurePage;
            }
            if (commandBytes[3] == 0x80)
            {
                return UmtCommandsEnum.ResearchHeaders;
            }

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

            return UmtCommandsEnum.StubCommand;
        }

        private string AddZerosToBinary(string binary)
        {
            if (binary.Length < 8)
            {
                while (binary.Length < 8)
                {
                    binary = binary.Insert(0, "0");
                }
            }
            return binary;
        }

        private void ExportByteData(UmtCommandsEnum commandName, byte[] message)
        {
            var points = GetPayload(message);
            ByteMessageReceived?.Invoke(commandName, points);
        }

        /// <summary>
        /// Словарь соответствия адрессов командам
        /// </summary>
        private Dictionary<byte[], UmtCommandsEnum> _commandDictionary = new Dictionary<byte[], UmtCommandsEnum>()
        {
            { new byte[]{ 0x00, 0x00, 0x00, 0x00}, UmtCommandsEnum.DeviceType },
            { new byte[]{ 0x02, 0x00, 0x00, 0x00}, UmtCommandsEnum.MemoryModelVersion },
            { new byte[]{ 0x04, 0x00, 0x00, 0x00}, UmtCommandsEnum.DeviceNameAddress },
            { new byte[]{ 0x08, 0x00, 0x00, 0x00}, UmtCommandsEnum.DeviceNameSize },
            { new byte[]{ 0x0A, 0x00, 0x00, 0x00}, UmtCommandsEnum.DeviceNumber },
            { new byte[]{ 0x00, 0x00, 0x00, 0x00}, UmtCommandsEnum.ProgrammVersionAddress },
            { new byte[]{ 0x04, 0x00, 0x00, 0x00}, UmtCommandsEnum.ProgrammVersionSize },
            { new byte[]{ 0x0E, 0x00, 0x00, 0x00}, UmtCommandsEnum.Adrtec },
            { new byte[]{ 0x10, 0x00, 0x00, 0x00}, UmtCommandsEnum.Ukstr },
            { new byte[]{ 0x12, 0x00, 0x00, 0x00}, UmtCommandsEnum.Ukbl},
            { new byte[]{ 0x14, 0x00, 0x00, 0x00}, UmtCommandsEnum.Page },
            { new byte[]{ 0x16, 0x00, 0x00, 0x00}, UmtCommandsEnum.Kolstr },
            { new byte[]{ 0x18, 0x00, 0x00, 0x00}, UmtCommandsEnum.Kolbl },
            { new byte[]{ 0x1A, 0x00, 0x00, 0x00}, UmtCommandsEnum.BaundRate },

            { new byte[]{ 0x00, 0x10, 0x00, 0x00}, UmtCommandsEnum.ProgrammVersionAddress },
            { new byte[]{ 0x04, 0x10, 0x00, 0x00}, UmtCommandsEnum.ProgrammVersionSize },

            { new byte[]{ 0x00, 0x20, 0x00, 0x00}, UmtCommandsEnum.Tabinv },

            { new byte[]{ 0x00, 0x80, 0x00, 0x00}, UmtCommandsEnum.Vissl },
            { new byte[]{ 0x01, 0x80, 0x00, 0x00}, UmtCommandsEnum.Kust },
            { new byte[]{ 0x06, 0x80, 0x00, 0x00}, UmtCommandsEnum.Skv },
            { new byte[]{ 0x0C, 0x80, 0x00, 0x00}, UmtCommandsEnum.Field },
            { new byte[]{ 0x0E, 0x80, 0x00, 0x00}, UmtCommandsEnum.Shop },
            { new byte[]{ 0x10, 0x80, 0x00, 0x00}, UmtCommandsEnum.Operator },
            { new byte[]{ 0x12, 0x80, 0x00, 0x00}, UmtCommandsEnum.Noldav },
            { new byte[]{ 0x16, 0x80, 0x00, 0x00}, UmtCommandsEnum.Interval },
            { new byte[]{ 0x1A, 0x80, 0x00, 0x00}, UmtCommandsEnum.Revbit },
            { new byte[]{ 0x1C, 0x80, 0x00, 0x00}, UmtCommandsEnum.Cher },

            { new byte[]{ 0x00, 0x84, 0x00, 0x00}, UmtCommandsEnum.Dav },
            { new byte[]{ 0x04, 0x84, 0x00, 0x00}, UmtCommandsEnum.Temp },
            { new byte[]{ 0x08, 0x84, 0x00, 0x00}, UmtCommandsEnum.ExTemp },
            { new byte[]{ 0x0C, 0x84, 0x00, 0x00}, UmtCommandsEnum.Acc },
            { new byte[]{ 0x10, 0x84, 0x00, 0x00}, UmtCommandsEnum.RaznR },
            { new byte[]{ 0x14, 0x84, 0x00, 0x00}, UmtCommandsEnum.ObshR },
            { new byte[]{ 0x18, 0x84, 0x00, 0x00}, UmtCommandsEnum.Emak },
            { new byte[]{ 0x1A, 0x84, 0x00, 0x00}, UmtCommandsEnum.Emem },
            { new byte[]{ 0x1C, 0x84, 0x00, 0x00}, UmtCommandsEnum.Kolisl },
            { new byte[]{ 0x1E, 0x84, 0x00, 0x00}, UmtCommandsEnum.Schstr },
            { new byte[]{ 0x20, 0x84, 0x00, 0x00}, UmtCommandsEnum.Schbl },
            { new byte[]{ 0x22, 0x84, 0x00, 0x00}, UmtCommandsEnum.Koliz },
            { new byte[]{ 0x26, 0x84, 0x00, 0x00}, UmtCommandsEnum.Time },
            { new byte[]{ 0x2C, 0x84, 0x00, 0x00}, UmtCommandsEnum.TIMEost },

            { new byte[]{ 0x00, 0x88, 0x00, 0x00}, UmtCommandsEnum.StartMeasurement },
            { new byte[]{ 0x02, 0x88, 0x00, 0x00}, UmtCommandsEnum.Sostdat},

            { new byte[]{ 0x00, 0x00, 0x00, 0x70}, UmtCommandsEnum.MeasurePage},
            { new byte[]{ 0x00, 0x00, 0x00, 0x80}, UmtCommandsEnum.ResearchHeaders},
        };
    }
}
