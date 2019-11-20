using Autofac;
using Autofac.Core;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services
{
    public static class SensorFactory
    {
        public static ISensor CreateSensor(ScannedDeviceInfo deviceInfo)
        {
            if (deviceInfo.Name.Contains("DDIN"))
            {
                var ddin2 = new Ddin2Sensor(deviceInfo,
                    AppContainer.Container.Resolve<IBluetoothClassicAdapter>(new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)));
                return ddin2;
            }
            return null;
        }
    }
}
