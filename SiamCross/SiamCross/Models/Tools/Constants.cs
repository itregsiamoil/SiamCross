using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Tools
{
    public static class Constants
    {
        public static Dictionary<string, string> GetQuickReportDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                { "BatteryVoltage", "" },
                { "Temperature", "" },
                { "LoadChanel", "" },
                { "AccelerationChanel", "" }
            };
            return dict;
        }
    }
}
