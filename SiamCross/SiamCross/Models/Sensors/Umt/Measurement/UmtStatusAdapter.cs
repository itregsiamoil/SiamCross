using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Umt.Measurement
{
    public class UmtStatusAdapter
    {
        public UmtMeasurementStatus StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return UmtMeasurementStatus.Empty;
                    case "1": return UmtMeasurementStatus.Measurement;
                }
            }

            return UmtMeasurementStatus.Empty;
        }

        public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return Resource.FreeStatus;
                    case "1": return Resource.MeasurementStatus;
                }
            }

            return Resource.FreeStatus; //stub
        }
    }
}
