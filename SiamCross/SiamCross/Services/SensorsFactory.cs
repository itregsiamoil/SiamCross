using SiamCross.Models;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services
{
    public static class SensorsFactory
    {
        public static ISensor CreateSensor(ScannedDeviceInfo deviceInfo)
        {
            if (deviceInfo.Name.Contains("DDIN"))
            {
                return new Ddin2Sensor();
            }
            return null;
        }
    }
}
