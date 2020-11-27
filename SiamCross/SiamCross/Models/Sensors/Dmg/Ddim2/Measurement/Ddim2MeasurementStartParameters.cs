using SiamCross.Models.Sensors.Dmg;

namespace SiamCross.Models.Sensors.Dmg.Ddim2.Measurement
{
    public class Ddim2MeasurementStartParameters : DmgBaseMeasureParameters
    {
        public Ddim2MeasurementStartParameters(
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
