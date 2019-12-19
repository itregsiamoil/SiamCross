using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2.Measurement
{
    public enum Ddin2MeasurementStatus
    {
        Empty = 0,
        Busy = 1,
        Calc = 2,
        Ready = 4,
        Error = 5
    }
}
