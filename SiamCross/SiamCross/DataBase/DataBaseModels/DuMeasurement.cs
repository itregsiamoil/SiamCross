using SiamCross.Models.Sensors.Du.Measurement;
using System;
using System.Globalization;

namespace SiamCross.DataBase.DataBaseModels
{
    public class DuMeasurement
    {
        public int Id { get; set; }
        public int SrcFluidLevel
        { get => MeasData.SrcFluidLevel; set => MeasData.SrcFluidLevel = (UInt16)value; }
        public int SrcReflectionsCount
        { get => MeasData.SrcReflectionsCount; set => MeasData.SrcReflectionsCount = (UInt16)value; }
        public float AnnularPressure
        { get => MeasData.AnnularPressure; set => MeasData.AnnularPressure = value; }
        public byte[] Echogram
        { get => MeasData.Echogram; set => MeasData.Echogram = value; }
        /*/ Secondary /*/
        public string SoundSpeed
        { get => MeasData.SecondaryParameters.SoundSpeed; set => MeasData.SecondaryParameters.SoundSpeed = value; }
        public string MeasurementType
        { get => MeasData.SecondaryParameters.MeasurementType; set => MeasData.SecondaryParameters.MeasurementType = value; }
        public string SoundSpeedCorrection
        { get => MeasData.SecondaryParameters.SoundSpeedCorrection; set => MeasData.SecondaryParameters.SoundSpeedCorrection = value; }
        public string Field
        { get => MeasData.SecondaryParameters.Field; set => MeasData.SecondaryParameters.Field = value; }
        public string Well
        { get => MeasData.SecondaryParameters.Well; set => MeasData.SecondaryParameters.Well = value; }
        public string Bush
        { get => MeasData.SecondaryParameters.Bush; set => MeasData.SecondaryParameters.Bush = value; }
        public string Shop
        { get => MeasData.SecondaryParameters.Shop; set => MeasData.SecondaryParameters.Shop = value; }
        public string BufferPressure
        { get => MeasData.SecondaryParameters.BufferPressure; set => MeasData.SecondaryParameters.BufferPressure = value; }
        public string Comment
        { get => MeasData.SecondaryParameters.Comment; set => MeasData.SecondaryParameters.Comment = value; }
        public string Name
        { get => MeasData.SecondaryParameters.DeviceName; set => MeasData.SecondaryParameters.DeviceName = value; }
        public string BatteryVolt
        { get => MeasData.SecondaryParameters.BatteryVolt; set => MeasData.SecondaryParameters.BatteryVolt = value; }
        public string Temperature
        { get => MeasData.SecondaryParameters.Temperature; set => MeasData.SecondaryParameters.Temperature = value; }
        public string MainFirmware
        { get => MeasData.SecondaryParameters.MainFirmware; set => MeasData.SecondaryParameters.MainFirmware = value; }
        public string RadioFirmware
        { get => MeasData.SecondaryParameters.RadioFirmware; set => MeasData.SecondaryParameters.RadioFirmware = value; }
        public string ReportTimestamp
        {
            get => MeasData.Date.ToString(CultureInfo.InvariantCulture);
            set => MeasData.Date = DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        public DuMeasurementData MeasData { get; set; }
        public DateTime DateTime
        { get => MeasData.Date; set => MeasData.Date = value; }
        public UInt16 FluidLevel => MeasData.FluidLevel;
        public UInt16 NumberOfReflections => MeasData.NumberOfReflections;

        public double PumpDepth //Глубина подвески насоса
        { get => MeasData.StartParam.PumpDepth; set => MeasData.StartParam.PumpDepth = value; }

        public DuMeasurement()
        {
            DuMeasurementSecondaryParameters sec_par = new DuMeasurementSecondaryParameters();
            DuMeasurementStartParameters start_par = new DuMeasurementStartParameters(false, false, false, sec_par, 0.0);
            MeasData = new DuMeasurementData(new DateTime(0), start_par, 0, 0, 0, new byte[3000]);
            MeasData.SecondaryParameters.ResearchType = "";
        }
        public DuMeasurement(DuMeasurementData meas_data)
        {
            MeasData = meas_data;
            MeasData.SecondaryParameters.ResearchType = "";
        }

    }//public class DuMeasurement
}
