namespace SiamCross.Models.Sensors.Dmg
{
    public enum DmgMeasureStatus
    {
        Empty = 0,
        Busy = 1,
        Calc = 2,
        Ready = 4,
        Error = 5
    }

    public static class DmgMeasureStatusAdapter
    {
        public static DmgMeasureStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return DmgMeasureStatus.Empty;
                    case "1": return DmgMeasureStatus.Busy;
                    case "2": return DmgMeasureStatus.Calc;
                    case "4": return DmgMeasureStatus.Ready;
                    case "5": return DmgMeasureStatus.Error;
                }
            }

            return DmgMeasureStatus.Empty; //stub
        }

        public static string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.Stat_Free;
                    case "1": return Resource.Stat_Meas;
                    case "2": return Resource.Stat_Calc;
                    case "4": return Resource.Stat_Complete;
                    case "5": return Resource.Stat_Error;
                }
            }

            return Resource.Stat_Free; //stub
        }

        public static string StatusToReport(DmgMeasureStatus status)
        {
            switch (status)
            {
                case DmgMeasureStatus.Empty: return Resource.Stat_Free;
                case DmgMeasureStatus.Busy: return Resource.Stat_Meas;
                case DmgMeasureStatus.Calc: return Resource.Stat_Calc;
                case DmgMeasureStatus.Ready: return Resource.Stat_Complete;
                default:
                case DmgMeasureStatus.Error: return Resource.Stat_Error;
            }
        }


        public static string CreateProgressStatus(int progress)
        {
            return Resource.Stat_Complete + $" {progress}%";
        }
    }
}
