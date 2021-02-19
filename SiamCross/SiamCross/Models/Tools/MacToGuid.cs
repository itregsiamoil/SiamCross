using System;

namespace SiamCross.Models.Tools
{
    public static class MacToGuid
    {
        public static bool TryConvert(string mac, out Guid giud)
        {
            string mac_no_delim = mac.ToUpper();
            int exist = mac_no_delim.IndexOf(':');
            //"00000000-0000-0000-0000-0016a4720012"
            while (0 < exist)
            {
                mac_no_delim = mac_no_delim.Remove(exist, 1);
                exist = mac_no_delim.IndexOf(':');
            }
            mac_no_delim = "00000000-0000-0000-0000-" + mac_no_delim;
            return Guid.TryParse(mac_no_delim, out giud);
        }

        public static Guid Convert(string mac)
        {
            if (TryConvert(mac, out Guid guid))
                return guid;
            return new Guid();
        }
    }
}
