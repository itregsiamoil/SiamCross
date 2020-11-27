using SiamCross.Models.Sensors.Dmg.Ddim2;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M
{
    public class SiddosA3MParser : Ddim2Parser
    {
        public SiddosA3MParser() : base()
        {
        }
        public SiddosA3MParser(FirmWaveQualifier deviceFirmWaveQualifier,
            bool isResponseCheck) : base(deviceFirmWaveQualifier, isResponseCheck)
        {
        }
    }
}
