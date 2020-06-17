using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Umt
{
    public enum UmtCommandsEnum
    {
        //Общие регистры
        DeviceType,
        MemoryModelVersion,
        DeviceNameAddress,
        DeviceNameSize,
        DeviceNumber,
        Adrtec,
        Ukstr,
        Ukbl,
        Page,
        Kolstr,
        Kolbl,
        BaundRate,
        //Справочные данные
        ProgrammVersionAddress,
        ProgrammVersionSize,
        //Таблица валидных блоков
        Tabinv,
        //Параметры прибора и исследования
        Vissl,
        Kust,
        Skv,
        Field,
        Shop,
        Operator,
        Noldav,
        Interval,
        Revbit,
        Cher,
        //Текущие данные
        Dav,
        Temp,
        ExTemp,
        Acc,
        RaznR,
        ObshR,
        Emak,
        Emem,
        Kolisl,
        Schstr,
        Schbl,
        Koliz,
        Time,
        TIMEost,
        //Операционные регистры
        StartMeasurement,
        StartNullatingPressureSensor,
        Initialize,
        StartNullatingDevice,
        OffDevice,
        Sostdat,
        //Прочее
        MeasurePage,
        ResearchHeaders,
        DeviceProgrammVersion,
        CurrentParametersFrame,
        StubCommand
    }
}
