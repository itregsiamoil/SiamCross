using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Du.Measurement;
using System;

namespace SiamCross.DataBase.DataBaseModels
{
    public class DuMeasurement
    {
        public int Id { get; set; }
        public UInt16 Urov { get; set; }
        public UInt16 Otr { get; set; }
        public int AnnularPressure { get; set; }
        public float SoundSpeed { get; set; }
        public Byte[] Echogram { get; set; }

        /*/ Secondary /*/
        public string Field { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comment { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }

        public DuMeasurement(DuMeasurementData measurementData,
            MeasurementSecondaryParameters secondaryParams)
        {
            Urov = (UInt16)measurementData.FluidLevel;
            Otr = (UInt16)measurementData.NumberOfReflections;
            Echogram = measurementData.Echogram.ToArray();
            AnnularPressure = measurementData.AnnularPressure;
            SoundSpeed = measurementData.SoundSpeed;

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
