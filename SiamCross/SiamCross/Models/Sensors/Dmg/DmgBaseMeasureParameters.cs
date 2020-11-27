using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DmgBaseMeasureParameters : BaseMeasureStartParameters
    {
        public int DynPeriod { get; }
        public int ApertNumber { get; }
        public int Imtravel { get; }
        public int ModelPump { get; }

        public DmgBaseMeasureParameters(
                                 float dynPeriod,
                                 int apertNumber,
                                 float imtravel,
                                 int modelPump,
                                 MeasurementSecondaryParameters secondaryParameters)
            :base(secondaryParameters)
        {
            DynPeriod = Convert.ToInt32(dynPeriod * 1000);
            ApertNumber = apertNumber;
            Imtravel = Convert.ToInt32(imtravel * 1000);
            ModelPump = modelPump;
        }
    }
}
