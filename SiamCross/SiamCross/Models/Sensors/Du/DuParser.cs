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

                Debug.WriteLine("DU: " + BitConverter.ToString(message) + "\n");

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
                    case DuCommandsEnum.EchogramData:
                        ExportByteData(DuCommandsEnum.EchogramData, message);
                        break;
                    case DuCommandsEnum.ResearchHeader:
                        ExportByteData(DuCommandsEnum.ResearchHeader, message);
                        Console.WriteLine("Read Measurement Report");
                        break;
                    case DuCommandsEnum.Revbit:
                        var payload = GetPayload(message);

                        Debug.WriteLine(BitConverter.ToString(payload));
                        Debug.WriteLine(AddZerosToBinary(Convert.ToString(payload[1], 2)));
                        Debug.WriteLine(AddZerosToBinary(Convert.ToString(payload[0], 2)));
                     
                        break;
                    case DuCommandsEnum.Pressure:
                        ExportByteData(DuCommandsEnum.Pressure, message);
                        break;
                        //case DuCommandsEnum.SensorState:
                        //    ExportByteData(DuCommandsEnum.SensorState, message);
                        //    break;
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
                Debug.WriteLine(BitConverter.ToString(_byteBuffer.Buffer.ToArray()));
                _byteBuffer = new ByteBuffer();
            }
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
            try
            {
                string result = "";
                var payloadBytes = GetPayload(message);

                var dataType = DefineDataType(message);
                switch (dataType)
                {
                    case DeviceRegistersTypes.Int32:
                        result = BitConverter.ToInt32(payloadBytes, 0).ToString();
                        break;
                    case DeviceRegistersTypes.Int16:
                        result = BitConverter.ToInt16(payloadBytes, 0).ToString();
                        Debug.WriteLine($"TO INT16 {BitConverter.ToString(payloadBytes)}");
                        break;
                    case DeviceRegistersTypes.Int8:
                        if(payloadBytes.Length == 1)
                        result = BitConverter.ToInt16(new byte[]{ payloadBytes[0], 0x00}, 0).ToString();
                        break;
                    case DeviceRegistersTypes.S16:
                        float value = BitConverter.ToInt16(payloadBytes, 0);
                        Debug.WriteLine($"TO S16 {BitConverter.ToString(payloadBytes)}");
                        result = (value / 10).ToString();
                        break;
                    case DeviceRegistersTypes.String:
                        if (payloadBytes.Length > 20)
                        {
                            result = Encoding.UTF8.GetString(payloadBytes);
                        }
                        else
                        {
                            result = Encoding.GetEncoding(1251).GetString(payloadBytes);
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
            switch(message[7])
            {
                case 0x00:                                  
                    {
                        switch (message[5])
                        {
                            case 0x00:                                          // Общие регистры
                                {
                                    switch (message[4])
                                    {
                                        case 0x00:
                                        case 0x02:
                                        case 0x08:
                                            return DeviceRegistersTypes.Int16;
                                        case 0x04:
                                        case 0x0A:
                                            return DeviceRegistersTypes.Int32;
                                    }
                                    break;
                                }
                            case 0x10:                                          // Справочные регистры
                                {
                                    switch(message[4])
                                    {
                                        case 0x00:
                                            return DeviceRegistersTypes.Int32;
                                        case 0x04:
                                            return DeviceRegistersTypes.Int16;
                                    }
                                }
                                break;
                            case 0x80:                                          // Параметры прибора и исследования
                                return DeviceRegistersTypes.Int16;
                            case 0x84:                                          // Текущие данные
                                return DeviceRegistersTypes.S16;                
                            case 0x88:                                          // Операционные регистры
                                {
                                    switch(message[4])
                                    {
                                        case 0x00:
                                            return DeviceRegistersTypes.Int8;
                                        case 0x02:
                                            return DeviceRegistersTypes.Int16;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case 0x80:                                                      // Заголовок исследования
                    return DeviceRegistersTypes.Report;
                case 0x81:                                                      // Данные исследования
                    return DeviceRegistersTypes.Graph;
            }

            return DeviceRegistersTypes.String;
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
                return DuCommandsEnum.EchogramData;
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
            { new byte[]{ 0x04, 0x84, 0x00, 0x00}, DuCommandsEnum.Pressure },
            { new byte[]{ 0x00, 0x88, 0x00, 0x00}, DuCommandsEnum.StartMeasurement }, // по данному адрессу ответ на 5 комманд           
            { new byte[]{ 0x02, 0x88, 0x00, 0x00}, DuCommandsEnum.SensorState },
            { new byte[]{ 0x00, 0x00, 0x00, 0x80}, DuCommandsEnum.ResearchHeader },
            { new byte[]{ 0x00, 0x00, 0x00, 0x81}, DuCommandsEnum.EchogramData }
        };
    }
}
