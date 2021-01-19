using System;

namespace SiamCross.Models.Sensors.Dmg.Ddin2.Measurement
{
    public class Ddin2MeasurementStartParameters : DmgBaseMeasureParameters
    {
        public int Rod { get; }
        public Ddin2MeasurementStartParameters(float rod,
                                 float dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
            : base(dynPeriod, apertNumber, imtravel, modelPump, secondaryParameters)
        {
            Rod = Convert.ToInt32(rod * 10);
        }

    }
}
