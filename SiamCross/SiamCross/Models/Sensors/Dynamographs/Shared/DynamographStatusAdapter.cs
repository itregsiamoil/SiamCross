using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dynamographs.Shared
{
    public class DynamographStatusAdapter
    {
        public DynamographMeasurementStatus StringStatusToEnum(string stringStatus)
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

        public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.FreeStatus; 
                    case "1": return Resource.MeasurementStatus;
                    case "2": return Resource.CalculationStatus;
                    case "4": return Resource.SavingStatus;
                    case "5": return Resource.SavingStatus;
                }
            }

            return Resource.FreeStatus; //stub
        }

        public string CreateProgressStatus(int progress)
        {
            return Resource.SavingStatus + $" {progress}%";
        }
    }
}
