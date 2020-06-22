using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Umt
{
    public class UmtQuickReportBuilder
    {
        private string _batteryVoltage;
        private string _pressure;
        private string _temperature;

        public string Temperature
        {
            get
            {
                return
                    _temperature != null ?
                        $"{Resource.Temperature}: "
                        + _temperature
                        + $", {Resource.DegCentigradeUnits}\n"
                    : "";
            }

            set
            {
                _temperature = value;
            }
        }

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
                    + $", {Resource.AtmosphereUnits}\n"
                : "";
            }

            set
            {
                _pressure = value;
            }
        }

        public string GetReport()
        {
            return BatteryVoltage + Temperature + Pressure;
        }
    }
}
