using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementReport : Ddim2.Measurement.Ddim2MeasurementReport
    {
        public SiddosA3MMeasurementReport(short maxWeight, short minWeight,
            short travel, short period, short step, short weightDiscr, short timeDiscr)
            : base(maxWeight, minWeight, travel, period, step, weightDiscr, timeDiscr)
        {
        }
    }
}
