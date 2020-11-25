using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementReport : Ddim2.Measurement.Ddim2MeasurementReport
    {
        public SiddosA3MMeasurementReport(UInt16 maxWeight, UInt16 minWeight,
            UInt16 travel, UInt16 period, UInt16 step, UInt16 weightDiscr, UInt16 timeDiscr)
            : base(maxWeight, minWeight, travel, period, step, weightDiscr, timeDiscr)
        {
        }
    }
}
