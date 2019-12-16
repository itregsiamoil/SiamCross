using SiamCross.Models.Sensors.Ddim2.Measurement;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddim2
{
    public class Ddim2StatusAdapter
    {
        public Ddim2MeasurementStatusState StringStatusToEnum(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch(stringStatus)
                {
                    case "0": return Ddim2MeasurementStatusState.Empty;
                    case "1": return Ddim2MeasurementStatusState.Busy;
                    case "2": return Ddim2MeasurementStatusState.Calc;
                    case "4": return Ddim2MeasurementStatusState.Ready;
                    case "5": return Ddim2MeasurementStatusState.Error;
                }
            }

            return Ddim2MeasurementStatusState.Empty; //stub
        }

        public string StringStatusToReport(string stringStatus)
        {
            if (!string.IsNullOrEmpty(stringStatus))
            {
                switch (stringStatus)
                {
                    case "0": return "Датчик свободен"; 
                    case "1": return "Старт измерения";
                    case "2": return "Расчет";
                    case "4": return "Экспорт";
                    case "5": return "Ошибка";
                }
            }

            return "Датчик свободен"; //stub
        }
    }
}
