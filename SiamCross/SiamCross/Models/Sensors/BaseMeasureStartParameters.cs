using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors
{
    public class BaseMeasureStartParameters
    {
        public MeasurementSecondaryParameters SecondaryParameters { get; }
        public BaseMeasureStartParameters(MeasurementSecondaryParameters secondaryParameters)
        {
            SecondaryParameters = secondaryParameters;
        }
    }//abstract class BaseMeasureStartParameters
}
