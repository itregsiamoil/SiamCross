namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    public enum Kind
    {
        Dynamogramm = 1,
        ValveTest = 2,
        RodWeight = 3,
    }

    public static class KindExtensions
    {
        public static string Title(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.Dynamogramm: return Resource.Dynamogram;
                case Kind.ValveTest: return Resource.ValveTest;
                case Kind.RodWeight: return Resource.RodWeight;
                default: return "UNKNOWN";
            }
        }
        public static string Info(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.Dynamogramm: return Resource.Attention;
                case Kind.ValveTest: return Resource.Attention;
                case Kind.RodWeight: return Resource.Attention;
                default: return "UNKNOWN";
            }
        }
        public static byte ToByte(this Kind enumValue)
        {
            return (byte)enumValue;
        }
    }
}
