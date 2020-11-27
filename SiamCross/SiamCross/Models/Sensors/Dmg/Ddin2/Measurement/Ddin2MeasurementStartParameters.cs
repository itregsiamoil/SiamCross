using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.Ddin2.Measurement
{
    public class Ddin2MeasurementStartParameters
    {
        public int Rod { get; }
        public int DynPeriod { get; }
        public int ApertNumber { get; }
        public int Imtravel { get; }
        public int ModelPump { get; }

        public MeasurementSecondaryParameters SecondaryParameters { get; }

        public Ddin2MeasurementStartParameters(float rod,
                                 float dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
        {
            Rod = Convert.ToInt32(rod * 10);
            DynPeriod = Convert.ToInt32(dynPeriod * 1000);
            ApertNumber = apertNumber;
            Imtravel = Convert.ToInt32(imtravel * 1000);
            ModelPump = modelPump;
            SecondaryParameters = secondaryParameters;
        }
    }
}
