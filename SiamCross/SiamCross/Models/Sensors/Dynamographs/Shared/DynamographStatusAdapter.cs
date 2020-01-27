using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dynamographs.Shared
{
    [Preserve(AllMembers = true)]
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
                    case "0": return "Датчик свободен"; 
                    case "1": return "Замер";
                    case "2": return "Расчет";
                    case "4": return "Экспорт";
                    case "5": return "Сохранение";
                }
            }

            return "Датчик свободен"; //stub
        }
    }
}
