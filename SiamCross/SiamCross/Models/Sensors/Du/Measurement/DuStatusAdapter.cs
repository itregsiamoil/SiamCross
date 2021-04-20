namespace SiamCross.Models.Sensors.Du.Measurement
{
    public static class DuStatusAdapter
    {
        public static DuStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return DuStatus.Empty;
                    case "1": return DuStatus.NoiseMeasurement;
                    case "2": return DuStatus.WaitingForClick;
                    case "3": return DuStatus.EсhoMeasurement;
                    case "4": return DuStatus.Сompleted;
                    case "5": return DuStatus.ValvePreparation;
                }
            }

            return DuStatus.Empty;
        }

        public static string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.Stat_Free;
                    case "1": return Resource.Stat_NoiseMeas;
                    case "2": return Resource.Stat_ClickWait;
                    case "3": return Resource.Stat_Meas;
                    case "4": return Resource.Stat_Complete;
                    case "5": return Resource.Stat_ValvePreparation;
                }
            }

            return Resource.Stat_Free; //stub
        }

        public static string StatusToString(DuStatus status)
        {
            switch (status)
            {
                default: break;
                case DuStatus.Empty: return Resource.Stat_Free;
                case DuStatus.NoiseMeasurement: return Resource.Stat_NoiseMeas;
                case DuStatus.WaitingForClick: return Resource.Stat_ClickWait;
                case DuStatus.EсhoMeasurement: return Resource.Stat_Meas;
                case DuStatus.Сompleted: return Resource.Stat_Complete;
                case DuStatus.ValvePreparation: return Resource.Stat_ValvePreparation;
            }
            return Resource.Stat_Free; //stub
        }

    }
}
