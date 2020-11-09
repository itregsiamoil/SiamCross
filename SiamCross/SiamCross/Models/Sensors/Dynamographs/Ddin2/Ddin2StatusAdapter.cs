using SiamCross.Models.Sensors.Ddin2.Measurement;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2StatusAdapter
    {
        public Ddin2MeasurementStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Ddin2MeasurementStatus.Empty;
                    case "1": return Ddin2MeasurementStatus.Busy;
                    case "2": return Ddin2MeasurementStatus.Calc;
                    case "4": return Ddin2MeasurementStatus.Ready;
                    case "5": return Ddin2MeasurementStatus.Error;
                }
            }

            return Ddin2MeasurementStatus.Empty; //stub
        }

        public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.PreparationStatus;
                    case "1": return Resource.MeasurementStatus;
                    case "2": return Resource.CalculationStatus;
                    case "4": return Resource.SavingStatus;
                    case "5": return Resource.SavingStatus;
                }
            }

            return Resource.FreeStatus; //stub
        }
    }
}
