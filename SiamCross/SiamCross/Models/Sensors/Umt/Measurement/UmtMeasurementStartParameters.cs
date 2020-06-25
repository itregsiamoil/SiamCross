using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Umt.Measurement
{
    public class UmtMeasurementStartParameters
    {
        public UmtMeasurementType MeasurementType { get; }
        public int Interval { get; }
        public bool IsTemperatureMeasure { get;  }
        public MeasurementSecondaryParameters SecondaryParameters { get; }
        public UmtMeasurementStartParameters(UmtMeasurementType type, int interval,
            bool isTempMeasure, MeasurementSecondaryParameters measurementSecondaryParameters)
        {
            MeasurementType = type;
            Interval = interval;
            IsTemperatureMeasure = isTempMeasure;
            SecondaryParameters = measurementSecondaryParameters;
        }
    }
}
