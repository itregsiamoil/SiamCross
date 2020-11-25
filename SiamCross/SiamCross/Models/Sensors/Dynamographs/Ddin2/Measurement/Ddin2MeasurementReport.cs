using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2.Measurement
{
    public class Ddin2MeasurementReport
    {
        public UInt16 MaxWeight { get; }
        public UInt16 MinWeight { get; }
        public UInt16 Travel { get; }
        public UInt16 Period { get; }
        public UInt16 Step { get; }
        public UInt16 WeightDiscr { get; }
        public UInt16 TimeDiscr { get; }

        public Ddin2MeasurementReport(UInt16 maxWeight,
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

            System.Diagnostics.Debug.WriteLine("Ddin2 Measurement HEADER: "
                + " MaxWeight = " + MaxWeight.ToString()
                + " MinWeight = " + MinWeight.ToString()
                + " Travel = " + Travel.ToString()
                + " Period = " + Period.ToString()
                + " Step = " + Step.ToString()
                + " WeightDiscr = " + WeightDiscr.ToString()
                + " TimeDiscr = " + TimeDiscr.ToString()
                ); ;
            
        }
    }
}
