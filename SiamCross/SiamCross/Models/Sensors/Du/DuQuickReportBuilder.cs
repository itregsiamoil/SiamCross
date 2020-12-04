using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Du
{
    public class DuQuickReportBuilder
    {
        private string _batteryVoltage;
        private string _pressure;

        public string BatteryVoltage
        {
            get
            {
                return 
                    _batteryVoltage != null ?
                        $"{Resource.Voltage}: "
                        + _batteryVoltage 
                        + $", {Resource.VoltsUnits}\n"
                    : "";
            }

            set
            {
                _batteryVoltage = value;
            }
        }

        public string Pressure
        {
            get
            {
                return _pressure != null ?
                    $"{Resource.Pressure}: " 
                    + _pressure 
                    + $"({Resource.KGFCMUnits})\n"
                : "";
            }

            set
            {
                _pressure = value;
            }
        }

        public string GetReport()
        {
            return /*BatteryVoltage + */ Pressure;
        }
    }
}
