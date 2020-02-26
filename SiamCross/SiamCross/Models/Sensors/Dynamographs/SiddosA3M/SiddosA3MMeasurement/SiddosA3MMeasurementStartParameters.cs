using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementStartParameters : Ddim2.Measurement.Ddim2MeasurementStartParameters
    {
        public SiddosA3MMeasurementStartParameters(int dynPeriod, int apertNumber,
            float imtravel, int modelPump, MeasurementSecondaryParameters secondaryParameters) 
            : base(dynPeriod, apertNumber, imtravel, modelPump, secondaryParameters)
        {
        }
    }
}
