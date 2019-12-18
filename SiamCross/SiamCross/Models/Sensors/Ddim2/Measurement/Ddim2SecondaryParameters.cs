using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddim2.Measurement
{
    public class MeasurementSecondaryParameters
    {
        public MeasurementSecondaryParameters(
            string field,
            string well,
            string bush,
            string shop,
            string bufferPressure,
            string comment)
        {
            Field = field;
            Well = well;
            Bush = bush;
            Shop = shop;
            BufferPressure = bufferPressure;
            Comment = comment;
        }

        public string Field { get; }
        public string Well { get; }
        public string Bush { get; }
        public string Shop { get; }
        public string BufferPressure { get; }
        public string Comment { get; }

    }
}
