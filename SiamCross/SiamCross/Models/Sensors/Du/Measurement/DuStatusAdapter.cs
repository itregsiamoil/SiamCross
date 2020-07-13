using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuStatusAdapter
    {
        public DuMeasurementStatus StringStatusToEnum(string stringStatus)
        {
            if(!string.IsNullOrEmpty(stringStatus))
            {
                switch(stringStatus)
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

        public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.FreeStatus;
                    case "1": return Resource.NoiseStatus;
                    case "2": return Resource.ClickStatus;
                    case "3": return Resource.MeasurementStatus;
                    case "4": return Resource.SavingStatus;
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
