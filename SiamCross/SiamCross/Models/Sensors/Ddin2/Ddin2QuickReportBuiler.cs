using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2
{ 
    public class Ddin2QuickReportBuiler
    {
        private string _batteryVoltage;
        private string _temperatute;
        private string _load;
        private string _acceleration;

        public string ZeroOffsetLoad { get; set; }
        public string SensitivityLoad { get; set; }

        public Ddin2QuickReportBuiler()
        {
            ZeroOffsetLoad = null;
            SensitivityLoad = null;
        }

        public string BatteryVoltage
        {
            get
            {
                return _batteryVoltage != null ?
                    "Напряжение: " + _batteryVoltage + ", В\n"
                    : "";
            }
            set
            {
                _batteryVoltage = value;
            }
        }

        public string Temperature
        {
            get
            {
                return _temperatute != null ?
                    "Температура: " + _temperatute + ", °C\n"
                    : "";
            }
            set
            {
                _temperatute = value;
            }
        }

        public bool IsKillosParametersReady
        {
            get => ZeroOffsetLoad != null && SensitivityLoad != null;
        }

        public string Load
        {
            get
            {
                if (_load != null)
                {
                    float load = float.Parse(_load);
                    if (IsKillosParametersReady)
                    {
                        return "Нагрузка: " + Math.Round(load, 2) + @", мВ \ " +
                            ((int)((load - float.Parse(ZeroOffsetLoad))
                                / float.Parse(SensitivityLoad))).ToString() +
                            ", КГ\n";
                    }
                    else
                    {
                        return "Нагрузка: " + Math.Round(load, 2) + ", мВ\n";
                    }
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _load = value;
            }
        }

        public string Acceleration
        {
            get
            {
                return _acceleration != null ?
                   "Ускорение: " + Math.Round(float.Parse(_acceleration), 2) + ", мВ"
                    : "";
            }
            set
            {
                _acceleration = value;
            }
        }

        public string GetReport()
        {
            return BatteryVoltage + Temperature + Load + Acceleration;
        }

        public void Clear()
        {
            BatteryVoltage = "";
            Temperature = "";
            Load = "";
            Acceleration = "";
        }
    }
}
