using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Commands
    {
        /// <summary>
        /// Готовые сообщения команд
        /// </summary>
        public static Dictionary<string, byte[]> FullCommandDictionary = new Dictionary<string, byte[]>()
        {
            // Общие регистры (0x00000000)

            /// Тип устройста
            ["DeviceType"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x90, 0x67
                },

            /// Версия модели памяти
            ["MemoryModelVersion"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x91, 0x85
                },

            /// Адресс имени устройста
            ["DeviceNameAddress"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x04, 0x00, 0x00, 0x00, 0x04, 0x00,
                    0x92, 0x43
                },

            /// Размер имени устройства
            ["DeviceNameSize"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x08, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x91, 0x2F
                },

            /// Номер устройства
            ["DeviceNumber"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x0A, 0x00, 0x00, 0x00, 0x04, 0x00,
                    0x93, 0x6D
                },

            // Информационные регистры (0x00001000)(r)

            /// Адресс версии программы
            ["ProgrammVersionAddress"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x00, 0x10, 0x00, 0x00, 0x04, 0x00,
                    0x52, 0x04
                },

            /// Размер версии программы
            ["ProgrammVersionSize"] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x04, 0x10, 0x00, 0x00, 0x02, 0x00,
                    0x50, 0x20
                },

            // Регистры параметров измерения (0x00008000)(r/w)

            ///  Диаметр штока
            ["Rod"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x91, 0xB9
            },

            ///  Период качания
            ["DynPeriod"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x80, 0x00, 0x00, 0x04, 0x00,
                0x93, 0xFB
            },

            ///  Номер отверстия
            ["ApertNumber"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x06, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x91, 0xDF
            },

            ///  Длинна хода
            ["Imtravel"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x90, 0xF1
            },

            ///  Тип ШГНУ
            ["ModelPump"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0A, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x91, 0x13
            },

            // Энергозависимые параметры (0x00008100) 

            ///  НКП датчика нагрузки
            ["SensorLoadNKP"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAF, 0xD9
            },

            ///  РКП датчика нагрузки
            ["SensorLoadRKP"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAE, 0x5D
            },

            ///  0g датчика ускорения
            ["SensorAcceleration0G"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAE, 0x91
            },

            ///  1g датчика ускорения
            ["SensorAcceleration1G"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0C, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAF, 0x15
            },

            ///  Интервал включения
            ["SwitchingInterval"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x10, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAD, 0x49
            },

            ///  Смещение нуля темп
            ["ZeroOffsetTemperature"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x14, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAC, 0xCD
            },

            ///  Коэф. наклона темп
            ["SlopeFactorTemperature"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x18, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAC, 0x01
            },

            /// -1g датчика ускорения
            ["SensorAccelerationMinus1G"] =
            new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1C, 0x81, 0x00, 0x00, 0x04, 0x00,
                0xAD, 0x85
            },

            /// Запрет выключения по времени
            ["EnableTimeOff"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x20, 0x81, 0x00, 0x00, 0x02, 0x00,
                0xAB, 0x19
            },

            /// Время выключения датчика
            ["TimeOff"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x22, 0x81, 0x00, 0x00, 0x02, 0x00,
                0xAA, 0xFB
            },

            // Текущие параметры (0x00008400)

            /// Напряжение аккумулятора
            ["BatteryVoltage"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x60, 0x79
            },

            /// Температура
            ["Тemperature"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x61, 0x9B
            },

            /// Канал нагрузки
            ["LoadChanel"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x62, 0x5D
            },

            /// Канал ускорения
            ["AccelerationChanel"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x62, 0x91
            },

            // Прочитать статус измерения
            ["ReadDeviceStatus"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x88, 0x00, 0x00, 0x02, 0x00,
                0x71, 0x9A
            },

            // Начать измерение
            ["StartMeasurement"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x02, 0x00,
                0x43, 0x78, //CRC1
                0x01, 0x00, //Данные
                0x00, 0x20  //CRC2
            },

            // Забрать отчет после измерения
            ["ReadMeasurementReport"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x80, 0x0E, 0x00,
                0x94, 0x8F
            },

            // Установить прибору статус "свободен"
            ["SetDeviceStatusFree"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x02, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x42, 0x6A, //CRC1
                0x00, 0x00, //Данные
                0x01, 0xB0  //CRC2
            },

            // Прочитать код ошибки измерения
            ["ReadMeasurementErrorCode"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x88, 0x00, 0x00, 0x02, 0x00,
                0x71, 0xFC
            },

            // Инциализация измерения
            ["InitializeMeasurement"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x02, 0x00,
                0x00, 0xD0
            },

            // Начальный адрес для чтения данных 
            // графика динамограммы. Без CRC
            ["ExportDynGraph"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x81, 0x14, 0x00
            },

            // Начальный адрес для чтения данных 
            // графика ускорения. Без CRC
            ["ExportAccelerationGraph"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x83, 0x02, 0x00
            },

            //Прочитать данные динамограммы,
            //1000 байтов
            ["GetDgmData"] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x81, 0xE8, 0x03,
                0xCF, 0x2E
            }
        };

        /// <summary>
        /// Адресса регистров памяти + размер данных
        /// </summary>
        public static Dictionary<string, byte[]> CommandRegistersDictionary =
            new Dictionary<string, byte[]>()
            {
                ["Operation"] =
                    new byte[] { 0x00, 0x88, 0x00, 0x00, 0x02, 0x00 },

                ["ProgramingInitialize"] =
                    new byte[] { 0x00, 0x89, 0x00, 0x00, 0x10, 0x00 },

                ["ProgramingSelf"] =
                    new byte[] { 0x10, 0x89, 0x00, 0x00, 0x02, 0x00 }
            };
    }
}
