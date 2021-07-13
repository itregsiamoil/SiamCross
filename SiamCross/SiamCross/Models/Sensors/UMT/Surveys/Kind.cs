namespace SiamCross.Models.Sensors.Umt.Surveys
{
    public enum Kind
    {
        Static = 1,
        Dynamic = 2,
        PeriodicStatic = 3,
        PeriodycDynamic = 4,
    }

    public static class KindExtensions
    {
        public static string Title(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.Static: return Resource.Static;
                case Kind.Dynamic: return Resource.Dynamics;
                case Kind.PeriodicStatic: return Resource.Static_period;
                case Kind.PeriodycDynamic: return Resource.Dynamics_period;
                default: return "UNKNOWN";
            }
        }
        public static string Info(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.Static: return Resource.AboutStaticAndDynamics;
                case Kind.Dynamic: return Resource.AboutStaticAndDynamics;
                case Kind.PeriodicStatic: return Resource.AboutStatic_period_Dynamics_period;
                case Kind.PeriodycDynamic: return Resource.AboutStatic_period_Dynamics_period;
                default: return "UNKNOWN";
            }
        }
        public static byte ToByte(this Kind enumValue)
        {
            return (byte)enumValue;
        }
    }
}
