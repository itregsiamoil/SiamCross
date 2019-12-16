﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddim2.Measurement
{
    public class Ddim2MeasurementReport
    {
        public short MaxWeight { get; }
        public short MinWeight { get; }
        public short Travel { get; }
        public short Period { get; }
        public short Step { get; }
        public short WeightDiscr { get; }
        public short TimeDiscr { get; }

        public Ddim2MeasurementReport(short maxWeight,
            short minWeight,
            short travel,
            short period,
            short step,
            short weightDiscr,
            short timeDiscr)
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
