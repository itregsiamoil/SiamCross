using System.Collections.Generic;

namespace SiamCross.Models.Sensors.Du
{
    public class DuCommands
    {
        /// <summary>
        /// Готовые сообщения команд
        /// </summary>
        public static Dictionary<DuCommandsEnum, byte[]> FullCommandDictionary = new Dictionary<DuCommandsEnum, byte[]>()
        {
            // Общие регистры (0x00000000)

            /// Тип устройста
            [DuCommandsEnum.DeviceType] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x90, 0x67
                },

            /// Версия модели памяти
            [DuCommandsEnum.MemoryModelVersion] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x91, 0x85
                },

            /// Адресс имени устройста
            [DuCommandsEnum.DeviceNameAddress] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x04, 0x00, 0x00, 0x00, 0x04, 0x00,
                    0x92, 0x43
                },

            /// Размер имени устройства
            [DuCommandsEnum.DeviceNameSize] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x08, 0x00, 0x00, 0x00, 0x02, 0x00,
                    0x91, 0x2F
                },

            /// Номер устройства
            [DuCommandsEnum.DeviceNumber] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x0A, 0x00, 0x00, 0x00, 0x04, 0x00,
                    0x93, 0x6D
                },

            // Информационные регистры (0x00001000)(r)

            /// Адресс версии программы
            [DuCommandsEnum.ProgrammVersionAddress] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x00, 0x10, 0x00, 0x00, 0x04, 0x00,
                    0x52, 0x04
                },

            /// Размер версии программы
            [DuCommandsEnum.ProgrammVersionSize] = new byte[]
                {
                    0x0D, 0x0A,
                    0x01, 0x01,
                    0x04, 0x10, 0x00, 0x00, 0x02, 0x00,
                    0x50, 0x20
                },

            // Регистры параметров измерения (0x00008000)(r/w)

            ///  Чувствительность датчика давления, 0.1мВ/100атм
            [DuCommandsEnum.PressureSensitivity] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x80,
                0x00, 0x00, 0x02, 0x00,
                0x91, 0xB9
            },

            ///  Чувствительность пьезодатчика в условных единицах от 0 до 255
            [DuCommandsEnum.PiezoelectricSensitivity] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x90, 0x5B
            },

            ///  Смещение нуля датчика давления , 0.1атм (только чтение)
            [DuCommandsEnum.PressureZeroOffset] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x90, 0x3D
            },

            ///  смещение нуля канала усиления сигнала с пьезодатчика (только чтение). 
            ///  В абсолютных единицах. 
            ///  При выводе на индикатор в уровнемере СУДОС-мастер извлекается корень квадратный.
            [DuCommandsEnum.PiezoelectricOffset] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x06, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x91, 0xDF
            },

            ///  Расшифровка битов REVBIT.
            ///  D6 - максимальный уровень при скорости звука 341м/с 0 – 3000м, 1 – 6000м.
            ///  D0=0 – выпуск газа из скважины. D0=1 – впуск газа в скважину (ГАИ-1, баллон с газом).
            ///  D9=0 – дополнительное усиление выключено.
            ///  D9=1 – дополнительное усиление включено.
            ///  Неиспользуемые биты должны быть равны нулю.
            [DuCommandsEnum.Revbit] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x90, 0xF1
            },

            // Текущие параметры (0x00008400)

            /// Напряжение аккумулятора 0.1В (только чтение)
            [DuCommandsEnum.Voltage] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x60, 0x79
            },

            /// Давление, 0.1атм (только чтение)
            [DuCommandsEnum.Pressure] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x61, 0xFD
            },

            // 0x00008800 – операционные регистры

            // Запуск измерения уровня
            [DuCommandsEnum.StartMeasurement] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x01,
                0x7E, 0x80
            },

            // Запуск обнуления датчика давления
            [DuCommandsEnum.StartZeroingPressure] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x02,
                0x3E, 0x81
            },

            // Запуск инициализации (для проведения инициализации послать команду дважды)
            [DuCommandsEnum.Initialize] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x03,
                0xFF, 0x41
            },

            // Обнуление состояния датчика SOSTDAT
            [DuCommandsEnum.StateZeroing] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x04,
                0xBE, 0x83
            },

            // Выключить датчик
            [DuCommandsEnum.SensorOff] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x43, 0x88,
                0x05,
                0x7F, 0x43
            },

            // Cостояние датчика SOSTDAT (только чтение):
            // 0 – датчик ничего не делает, 
            // 1 – датчик измеряет шумы в скважине в течение 1 секунды после запуска измерения уровня,
            // 2 – датчик ожидает нажатия на ручной клапан, 
            // 3 – производится запись эхограммы 18 секунд для 3000метров и 36 секунд для 6000 метров,
            // 4 – измерение уровня закончено, но не считано 
            // (SOSTDAT=4 сохраняется во фрам и не стирается при выключении питания).
            [DuCommandsEnum.SensorState] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x88, 0x00, 0x00, 0x02, 0x00,
                0x71, 0x9A
            },

            // Заголовок исследования
            [DuCommandsEnum.ResearchHeader] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x80, 0x04, 0x00,
                0x92, 0x2F
            },

        };

        /// <summary>
        /// Адресса регистров памяти
        /// без начала сообщения по протоколу, без размера данных, без crc
        /// </summary>
        public static Dictionary<DuCommandsEnum, byte[]> CommandRegistersDictionary =
            new Dictionary<DuCommandsEnum, byte[]>()
            {
                [DuCommandsEnum.EchogramData] = new byte[]
            {
                0x00, 0x00, 0x00, 0x81
            },
            };
    }
}
