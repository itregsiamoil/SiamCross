using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.Shared
{
    public enum DynamographMeasurementStatus
    {
        Empty = 0,
        Busy = 1,
        Calc = 2,
        Ready = 4,
        Error = 5
    }
}
