using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Du.Measurement;
using System;

namespace SiamCross.DataBase.DataBaseModels
{
    public class DuMeasurement
    {
        public int Id { get; set; }
        public int FluidLevel { get; set; }
        public int NumberOfReflections { get; set; }
        public int AnnularPressure { get; set; }
        public byte[] Echogram { get; set; }

        /*/ Secondary /*/
        public string SoundSpeed { get; set; }
        public string MeasurementType { get; set; }
        public string SoundSpeedCorrection { get; set; }
        public string Field { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comment { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }

        public DuMeasurement(DuMeasurementData measurementData)
        {
            FluidLevel = measurementData.FluidLevel;
            NumberOfReflections = measurementData.NumberOfReflections;
            Echogram = measurementData.Echogram.ToArray();
            AnnularPressure = measurementData.AnnularPressure;

            var secondaryParams = measurementData.SecondaryParameters;
            SoundSpeed = secondaryParams.SoundSpeed;
            MeasurementType = secondaryParams.MeasurementType;
            //SoundSpeedCorrection = secondaryParams;
            DateTime = measurementData.Date;
            Field = secondaryParams.Field;
            Well = secondaryParams.Well;
            Bush = secondaryParams.Bush;
            Shop = secondaryParams.Shop;
            BufferPressure = secondaryParams.BufferPressure;
            Comment = secondaryParams.Comment;
            Name = secondaryParams.DeviceName;
        }
    }
}
