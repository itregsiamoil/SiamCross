namespace SiamCross.Models.Sensors.Du.Measurement
{
    public enum DuStatus
    {
        Empty = 0,
        NoiseMeasurement = 1,
        WaitingForClick = 2,
        EсhoMeasurement = 3,
        Сompleted = 4,
        ValvePreparation = 5
    }
    public static class DuStatusExtensions
    {
        public static string Title(this DuStatus enumValue)
        {
            switch (enumValue)
            {
                case DuStatus.Empty: return Resource.Stat_Free;
                case DuStatus.NoiseMeasurement: return Resource.Stat_NoiseMeas;
                case DuStatus.WaitingForClick: return Resource.Stat_ClickWait;
                case DuStatus.EсhoMeasurement: return Resource.Stat_Meas;
                case DuStatus.Сompleted: return Resource.Stat_Complete;
                case DuStatus.ValvePreparation: return Resource.Stat_ValvePreparation;
                default: return "UNKNOWN";
            }
        }
        public static ushort ToUShort(this DuStatus enumValue)
        {
            return (ushort)enumValue;
        }
    }
}
