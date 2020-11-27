using System;

namespace SiamCross.Models.Sensors.Dmg.Ddim2.Measurement
{
    public class Ddim2MeasurementStartParameters
    {
        public int DynPeriod { get; }
        public int ApertNumber { get; }
        public int Imtravel { get; }
        public int ModelPump { get; }

        public MeasurementSecondaryParameters SecondaryParameters { get; }

        public Ddim2MeasurementStartParameters(
                                 float dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
        {
            DynPeriod = Convert.ToInt32(dynPeriod * 1000);
            ApertNumber = apertNumber;
            Imtravel = Convert.ToInt32(imtravel * 1000);
            ModelPump = modelPump;
            SecondaryParameters = secondaryParameters;
        }
    }
}
