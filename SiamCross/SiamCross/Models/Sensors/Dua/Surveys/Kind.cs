namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public enum Kind
    {
        LStatic = 1,
        LDynamic = 2,
        LRC = 3,
        LDC = 4,
        PAR = 5
    }

    public static class KindExtensions
    {
        public static string Title(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.LStatic: return Resource.StaticLevel;
                case Kind.LDynamic: return Resource.DynamicLevel;
                case Kind.LRC: return Resource.LRC;
                case Kind.LDC: return Resource.LDC;
                case Kind.PAR: return Resource.PAR;
                default: return "UNKNOWN";
            }
        }
        public static string Info(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.LStatic: return Resource.AboutStaticLevel;
                case Kind.LDynamic: return Resource.AboutDynamicLevel;
                case Kind.LRC: return Resource.AboutLRC;
                case Kind.LDC: return Resource.AboutLDC;
                case Kind.PAR: return Resource.AboutPAR;
                default: return "UNKNOWN";
            }
        }
        public static byte ToByte(this Kind enumValue)
        {
            return (byte)enumValue;
        }
    }
}
