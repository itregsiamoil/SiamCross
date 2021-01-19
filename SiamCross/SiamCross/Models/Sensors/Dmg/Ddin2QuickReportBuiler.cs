using System;


namespace SiamCross.Models.Sensors.Dmg
{
    public class DmgBaseQuickReportBuiler
    {
        private string _batteryVoltage;
        private string _temperatute;
        private string _load;
        private string _acceleration;

        public string ZeroOffsetLoad { get; set; }
        public string SensitivityLoad { get; set; }

        public DmgBaseQuickReportBuiler()
        {
            ZeroOffsetLoad = null;
            SensitivityLoad = null;
        }

        public string BatteryVoltage
        {
            get => _batteryVoltage != null ?
                    $"{Resource.Voltage}: " + _batteryVoltage + $", {Resource.VoltsUnits}\n"
                    : "";
            set => _batteryVoltage = value;
        }

        public string Temperature
        {
            get => _temperatute != null ?
                    $"{Resource.Temperature}: " + _temperatute + $", {Resource.DegCentigradeUnits}\n"
                    : "";
            set => _temperatute = value;
        }

        public bool IsKillosParametersReady => ZeroOffsetLoad != null && SensitivityLoad != null;

        public string Load
        {
            get
            {
                if (_load != null)
                {
                    float load = float.Parse(_load);
                    if (IsKillosParametersReady)
                    {
                        return $"{Resource.Load}: " + Math.Round(load, 2) + $", {Resource.MilliVoltsUnits}" + @" \ " +
                            ((int)((load - float.Parse(ZeroOffsetLoad))
                                / float.Parse(SensitivityLoad))).ToString() +
                            $", {Resource.Kilograms}\n";
                    }
                    else
                    {
                        return $"{Resource.Load}: " + Math.Round(load, 2) + $", {Resource.MilliVoltsUnits}\n";
                    }
                }
                else
                {
                    return "";
                }
            }
            set => _load = value;
        }

        public string Acceleration
        {
            get => _acceleration != null ?
                   $"{Resource.Acceleration}: " + Math.Round(float.Parse(_acceleration), 2) + $", {Resource.MilliVoltsUnits}"
                    : "";
            set => _acceleration = value;
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
