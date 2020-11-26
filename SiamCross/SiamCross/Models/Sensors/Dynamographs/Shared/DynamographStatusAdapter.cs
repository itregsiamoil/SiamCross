using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.Shared
{
    static public class DynamographStatusAdapter
    {
        static public DynamographMeasurementStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch(stringStatus)
                {
                    case "0": return DynamographMeasurementStatus.Empty;
                    case "1": return DynamographMeasurementStatus.Busy;
                    case "2": return DynamographMeasurementStatus.Calc;
                    case "4": return DynamographMeasurementStatus.Ready;
                    case "5": return DynamographMeasurementStatus.Error;
                }
            }

            return DynamographMeasurementStatus.Empty; //stub
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

        static public string StatusToReport(DynamographMeasurementStatus status)
        {
            switch (status)
            {
                case DynamographMeasurementStatus.Empty: return Resource.FreeStatus;
                case DynamographMeasurementStatus.Busy: return Resource.MeasurementStatus;
                case DynamographMeasurementStatus.Calc: return Resource.CalculationStatus;
                case DynamographMeasurementStatus.Ready: return Resource.SavingStatus;
                default:
                case DynamographMeasurementStatus.Error: return Resource.StatusSaveError;
            }
        }


        static public string CreateProgressStatus(int progress)
        {
            return Resource.SavingStatus + $" {progress}%";
        }
    }
}
