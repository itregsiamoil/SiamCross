using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors
{
    [Preserve(AllMembers = true)]
    public class MeasurementSecondaryParameters
    {
        public MeasurementSecondaryParameters(
            string name,
            string measurementType,
            string field,
            string well,
            string bush,
            string shop,
            string bufferPressure,
            string comment)
        {
            DeviceName = name;
            MeasurementType = measurementType;
            Field = field;
            Well = well;
            Bush = bush;
            Shop = shop;
            BufferPressure = bufferPressure;
            Comment = comment;
        }

        public string DeviceName { get; }
        public string MeasurementType { get; }
        public string Field { get; }
        public string Well { get; }
        public string Bush { get; }
        public string Shop { get; }
        public string BufferPressure { get; }
        public string Comment { get; }

    }
}
