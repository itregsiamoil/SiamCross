using SiamCross.Models.Sensors.Dmg;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M.Measurement
{
    public class SiddosA3MMeasurementStartParameters : DmgBaseMeasureParameters
    {
        public SiddosA3MMeasurementStartParameters(
                                 float dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
            : base(dynPeriod, apertNumber, imtravel, modelPump, secondaryParameters)
        {
        }
    }
}
