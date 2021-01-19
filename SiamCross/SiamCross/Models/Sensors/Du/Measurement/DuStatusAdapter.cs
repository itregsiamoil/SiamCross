namespace SiamCross.Models.Sensors.Du.Measurement
{
    public static class DuStatusAdapter
    {
        public static DuMeasurementStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return DuMeasurementStatus.Empty;
                    case "1": return DuMeasurementStatus.NoiseMeasurement;
                    case "2": return DuMeasurementStatus.WaitingForClick;
                    case "3": return DuMeasurementStatus.EсhoMeasurement;
                    case "4": return DuMeasurementStatus.Сompleted;
                }
            }

            return DuMeasurementStatus.Empty;
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
                }
            }

            return Resource.Stat_Free; //stub
        }

        public static string StatusToString(DuMeasurementStatus status)
        {
            switch (status)
            {
                default: break;
                case DuMeasurementStatus.Empty: return Resource.Stat_Free;
                case DuMeasurementStatus.NoiseMeasurement: return Resource.Stat_NoiseMeas;
                case DuMeasurementStatus.WaitingForClick: return Resource.Stat_ClickWait;
                case DuMeasurementStatus.EсhoMeasurement: return Resource.Stat_Meas;
                case DuMeasurementStatus.Сompleted: return Resource.Stat_Complete;
            }
            return Resource.Stat_Free; //stub
        }

    }
}
