using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Umt
{
    public class UmtCommands
    {
        public static Dictionary<UmtCommandsEnum, byte[]> FullCommandDictionary = new Dictionary<UmtCommandsEnum, byte[]>()
        {
            //Общие регистры 0x00000000

            //Тип устройства
            [UmtCommandsEnum.DeviceType] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x90, 0x67
            },

            //Версия модели данных
            [UmtCommandsEnum.MemoryModelVersion] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x91, 0x85
            },

            //адрес названия прибора 
            [UmtCommandsEnum.DeviceNameAddress] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x92, 0x43
            },

            //размер названия прибора 
            [UmtCommandsEnum.DeviceNameSize] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x91, 0x2F
            },

            //заводской номер 
            [UmtCommandsEnum.DeviceNumber] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0A, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x93, 0x6D
            },

            //адрес последней (текущей) записи в cтранице 
            [UmtCommandsEnum.Adrtec] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0E, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x91, 0x49
            },

            //адрес последней (текущей) страницы в флэш памяти 
            [UmtCommandsEnum.Ukstr] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x10, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x92, 0xF7
            },

            //адрес последнего (текущего) блока в флэш памяти 
            [UmtCommandsEnum.Ukbl] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x12, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x93, 0x15
            },

            //размер страницы, например 528 или 2112  
            [UmtCommandsEnum.Page] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x14, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x93, 0x73
            },

            //количество страниц в блоке, например 8192 или 64
            [UmtCommandsEnum.Kolstr] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x16, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x92, 0x91
            },

            //количество блоков в флэш памяти, например 1 или 8192
            [UmtCommandsEnum.Kolbl] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x18, 0x00, 0x00, 0x00, 0x02, 0x00,
                0x93, 0xBF
            },

            //скорость обмена по таблице
            [UmtCommandsEnum.BaundRate] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1A, 0x00, 0x00, 0x00, 0x01, 0x00,
                0x92, 0xAD
            },

            //Справочные данные 

            //адрес строки с версией программы 
            [UmtCommandsEnum.ProgrammVersionAddress] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x10, 0x00, 0x00, 0x04, 0x00,
                0x52, 0x04
            },

            //размер строки с версией
            [UmtCommandsEnum.ProgrammVersionSize] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x10, 0x00, 0x00, 0x02, 0x00,
                0x50, 0x20
            },

            //Таблица валидных блоков
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! НЕ Использовать!
            [UmtCommandsEnum.Tabinv] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x20, 0x00, 0x00, 0x00, 0x00,
                0x50, 0x20
            },

            //Параметры прибора и исследования

            //вид исследования от 1 до 4
            [UmtCommandsEnum.Vissl] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x80, 0x00, 0x00, 0x01, 0x00,
                0x91, 0x49
            },

            //номер куста
            [UmtCommandsEnum.Kust] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x01, 0x80, 0x00, 0x00, 0x05, 0x00,
                0x92, 0x58
            },

            //номер скважины
            [UmtCommandsEnum.Skv] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x06, 0x80, 0x00, 0x00, 0x06, 0x00,
                0x93, 0x1F
            },

            //код месторождения
            [UmtCommandsEnum.Field] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0C, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x91, 0x75
            },

            //номер цеха
            [UmtCommandsEnum.Shop] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0E, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x90, 0x97
            },

            //номер оператора
            [UmtCommandsEnum.Operator] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x10, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x93, 0x29
            },

            //смещение нуля датчика давления
            [UmtCommandsEnum.Noldav] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x12, 0x80, 0x00, 0x00, 0x04, 0x00,
                0x91, 0x6B
            },

            //интервал замера
            [UmtCommandsEnum.Interval] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x16, 0x80, 0x00, 0x00, 0x04, 0x00,
                0x90, 0xEF
            },

            //Ревбит
            [UmtCommandsEnum.Revbit] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1A, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x93, 0x83
            },

            //количество точек для видов исследования «3», «4»
            [UmtCommandsEnum.Cher] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1C, 0x80, 0x00, 0x00, 0x02, 0x00,
                0x93, 0xE5
            },

            //Текущие данные

            //давление 
            [UmtCommandsEnum.Dav] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x63, 0xD9
            },

            //температура внутренняя 
            [UmtCommandsEnum.Temp] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x04, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x62, 0x5D
            },

            //температура внешняя 
            [UmtCommandsEnum.ExTemp] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x08, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x62, 0x91
            },

            //напряжение аккумулятора 
            [UmtCommandsEnum.Acc] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x0C, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x63, 0x15
            },

            //разность сопротивлений моста
            [UmtCommandsEnum.RaznR] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x10, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x61, 0x49
            },

            //общее сопротивление моста
            [UmtCommandsEnum.ObshR] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x14, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x60, 0xCD
            },

            //емкость аккумулятора
            [UmtCommandsEnum.Emak] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x18, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x63, 0xA1
            },

            //количество свободной памяти
            [UmtCommandsEnum.Emem] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1A, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x62, 0x43
            },

            //количество исследований в памяти 
            [UmtCommandsEnum.Kolisl] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1C, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x62, 0x25
            },

            //количество свободных страниц в памяти
            [UmtCommandsEnum.Schstr] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x1E, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x63, 0xC7
            },

            //количество свободных блоков в памяти
            [UmtCommandsEnum.Schbl] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x20, 0x84, 0x00, 0x00, 0x02, 0x00,
                0x67, 0x19
            },

            //количество свободных измерений в памяти
            [UmtCommandsEnum.Koliz] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x22, 0x84, 0x00, 0x00, 0x04, 0x00,
                0x65, 0x5B
            },

            //время и дата
            [UmtCommandsEnum.Time] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x26, 0x84, 0x00, 0x00, 0x06, 0x00,
                0x65, 0xBF
            },

            //сколько осталось времени до измерения при повторных замерах
            [UmtCommandsEnum.TIMEost] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x2C, 0x84, 0x00, 0x00, 0x03, 0x00,
                0x66, 0x45
            },

            //Операционные регистры

            //запуск измерения 
            //!!!
            [UmtCommandsEnum.StartMeasurement] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x01,
                0x08, 0x31
            },

            //запуск обнуления датчика давления 
            //!!!
                    [UmtCommandsEnum.StartNullatingPressureSensor] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x02,
                0x48, 0x30
            },

            //Запуск инициализации
            [UmtCommandsEnum.Initialize] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x03,
                0x89, 0xF0
            },

            //Запуск обнуления датчика
            [UmtCommandsEnum.StartNullatingDevice] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x04,
                0xC8, 0x32
            },

            //Выключение датчика
            [UmtCommandsEnum.OffDevice] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x02,
                0x00, 0x88, 0x00, 0x00, 0x01, 0x00,
                0x05,
                0x09, 0xF2
            },

            //состояние датчика
            [UmtCommandsEnum.Sostdat] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x02, 0x88, 0x00, 0x00, 0x02, 0x00,
                0x71, 0x9A
            },

            //фрейм с текущими данными
            [UmtCommandsEnum.CurrentParametersFrame] = new byte[]
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x84, 0x00, 0x00, 0x10, 0x00,
                0x6C, 0xD9
            },
        };

        /// <summary>
        /// Адресса регистров памяти
        /// без начала сообщения по протоколу, без размера данных, без crc
        /// </summary>
        public static Dictionary<UmtCommandsEnum, byte[]> CommandRegistersDictionary =
            new Dictionary<UmtCommandsEnum, byte[]>()
            {
                //страница последнего измерения из фрам памяти, которая может быть частично занята измерениями
                [UmtCommandsEnum.MeasurePage] = new byte[]
            {
                0x00, 0x00, 0x00, 0x70
            },

                //Заголовки исследований
                [UmtCommandsEnum.ResearchHeaders] = new byte[]
            {
                0x00, 0x00, 0x00, 0x80,
            }
            };
    }
}
