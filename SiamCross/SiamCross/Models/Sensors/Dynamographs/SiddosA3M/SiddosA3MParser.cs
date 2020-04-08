using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M
{
    public class SiddosA3MParser : Ddim2Parser
    {
        public SiddosA3MParser(FirmWaveQualifier deviceFirmWaveQualifier,
            bool isResponseCheck) : base(deviceFirmWaveQualifier, isResponseCheck)
        {
        }
    }
}
