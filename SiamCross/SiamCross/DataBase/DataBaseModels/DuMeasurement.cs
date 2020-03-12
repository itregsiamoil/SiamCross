using SiamCross.Models.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.DataBase.DataBaseModels
{
    public class DuMeasurement
    {
        public int Id { get; set; }
        public UInt16 Urov { get; set; }
        public UInt16 Otr { get; set; }
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

        public DuMeasurement(UInt16 urov, UInt16 otr, Byte[] echogram,
            DateTime date,
            MeasurementSecondaryParameters secondaryParams)
        {
            Urov = urov;
            Otr = otr;
            Echogram = echogram;
            DateTime = date;
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
