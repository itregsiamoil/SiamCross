using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public enum DuMeasurementStatus
    {
        Empty = 0,
        NoiseMeasurement,
        WaitingForClick,
        EсhoMeasurement,
        Сompleted
    }
}
