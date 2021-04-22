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
                case Kind.Static: return "Статика";
                case Kind.Dynamic: return "Динамика";
                case Kind.PeriodicStatic: return "Статика(период)";
                case Kind.PeriodycDynamic: return "Динамика(период)";
                default: return "UNKNOWN";
            }
        }
        public static string Info(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.Static: return "единичное измерение";
                case Kind.Dynamic: return "единичное измерение";
                case Kind.PeriodicStatic: return "периодическое измерение";
                case Kind.PeriodycDynamic: return "периодическое измерение";
                default: return "UNKNOWN";
            }
        }
        public static byte ToByte(this Kind enumValue)
        {
            return (byte)enumValue;
        }
    }
}
