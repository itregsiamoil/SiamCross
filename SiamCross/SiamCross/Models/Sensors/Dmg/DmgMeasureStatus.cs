using System;
using System.Collections.Generic;
using System.Text;

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

    static public class DmgMeasureStatusAdapter
    {
        static public DmgMeasureStatus StringStatusToEnum(string stringStatus)
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

        static public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.FreeStatus;
                    case "1": return Resource.MeasurementStatus;
                    case "2": return Resource.CalculationStatus;
                    case "4": return Resource.SavingStatus;
                    case "5": return Resource.StatusSaveError;
                }
            }

            return Resource.FreeStatus; //stub
        }

        static public string StatusToReport(DmgMeasureStatus status)
        {
            switch (status)
            {
                case DmgMeasureStatus.Empty: return Resource.FreeStatus;
                case DmgMeasureStatus.Busy: return Resource.MeasurementStatus;
                case DmgMeasureStatus.Calc: return Resource.CalculationStatus;
                case DmgMeasureStatus.Ready: return Resource.SavingStatus;
                default:
                case DmgMeasureStatus.Error: return Resource.StatusSaveError;
            }
        }


        static public string CreateProgressStatus(int progress)
        {
            return Resource.SavingStatus + $" {progress}%";
        }
    }
}
