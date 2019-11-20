using Autofac;
using Autofac.Core;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddim2;
using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.AppObjects;
using Autofac;
using SiamCross.Models.Adapters;

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
            else if(deviceInfo.Name.Contains("DDIM"))
            {
                if (deviceInfo.BluetoothType == BluetoothType.Le)
                {
                    return new Ddim2Sensor(
                        AppContainer.Container.Resolve<IBluetoothLeAdapter>
                        (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                        new SensorData(SensorService.Instance.SensorsCount,
                        deviceInfo.Name, "Динамограф", ""));
                }
            }

            return null;
        }
    }
}
