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
                case Kind.LStatic: return "Статический уровень";
                case Kind.LDynamic: return "Динамический уровень";
                case Kind.LRC: return "КВУ";
                case Kind.LDC: return "КПУ";
                case Kind.PAR: return "АРД";
                default: return "UNKNOWN";
            }
        }
        public static string Info(this Kind enumValue)
        {
            switch (enumValue)
            {
                case Kind.LStatic: return "уровень жидкости в спокойном состоянии";
                case Kind.LDynamic: return "уровень жидкости, наблюдаемый при откачке";
                case Kind.LRC: return "кривая восстановления уровня";
                case Kind.LDC: return "кривая падения уровня";
                case Kind.PAR: return "автоматическая регистрация давления";
                default: return "UNKNOWN";
            }
        }
        public static ushort ToUShort(this Kind enumValue)
        {
            return (ushort)enumValue;
        }
    }
}
