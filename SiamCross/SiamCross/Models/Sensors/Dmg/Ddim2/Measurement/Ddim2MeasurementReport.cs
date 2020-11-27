using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.Ddim2.Measurement
{
    public class Ddim2MeasurementReport
    {
        public UInt16 MaxWeight { get; }
        public UInt16 MinWeight { get; }
        public UInt16 Travel { get; }
        public UInt16 Period { get; }
        public UInt16 Step { get; }
        public UInt16 WeightDiscr { get; }
        public UInt16 TimeDiscr { get; }

        public Ddim2MeasurementReport(UInt16 maxWeight,
            UInt16 minWeight,
            UInt16 travel,
            UInt16 period,
            UInt16 step,
            UInt16 weightDiscr,
            UInt16 timeDiscr)
        {
            MaxWeight = maxWeight;
            MinWeight = minWeight;
            Travel = travel;
            Period = period;
            Step = step;
            WeightDiscr = weightDiscr;
            TimeDiscr = timeDiscr;
        }
    }
}
