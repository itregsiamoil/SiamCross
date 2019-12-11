using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2.Measurement
{
    public struct Ddim2MeasurementParameters
    {
        public int Rod { get; }
        public int DynPeriod { get; }
        public int ApertNumber { get; }
        public int Imtravel { get; }
        public int ModelPump { get; }

        public Ddim2MeasurementParameters(int rod,
                                 int dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump)
        {
            Rod = rod * 10;
            DynPeriod = dynPeriod * 1000;
            ApertNumber = apertNumber;
            Imtravel = Convert.ToInt32(imtravel * 1000);
            ModelPump = modelPump;
        }
    }
}
