using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement
{
    [Preserve(AllMembers = true)]
    public class Ddim2MeasurementStartParameters
    {
        public int Rod { get; }
        public int DynPeriod { get; }
        public int ApertNumber { get; }
        public int Imtravel { get; }
        public int ModelPump { get; }

        public MeasurementSecondaryParameters SecondaryParameters { get; }

        public Ddim2MeasurementStartParameters(int rod,
                                 int dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
        {
            Rod = rod * 10;
            DynPeriod = dynPeriod * 1000;
            ApertNumber = apertNumber;
            Imtravel = Convert.ToInt32(imtravel * 1000);
            ModelPump = modelPump;
            SecondaryParameters = secondaryParameters;
        }
    }
}
