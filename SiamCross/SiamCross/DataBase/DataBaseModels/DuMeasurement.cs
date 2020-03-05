using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.DataBase.DataBaseModels
{
    public class DuMeasurement
    {
        public int Id { get; set; }

        public UInt16 Urov { get; set; }

        public UInt16 Otr { get; set; }

        public Byte[] Echogram { get; set; }
    }
}
