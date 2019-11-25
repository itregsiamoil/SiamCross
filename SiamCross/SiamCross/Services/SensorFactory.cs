﻿using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddim2;
using SiamCross.Models.Sensors.Ddin2;

namespace SiamCross.Services
{
    public static class SensorFactory
    {
        private static readonly string _initString = "Напряжение: \n Температура: \n Нагрузка: \n Ускорение";
        public static ISensor CreateSensor(ScannedDeviceInfo deviceInfo)
        {
            if (deviceInfo.Name.Contains("DDIN"))
            {
                var ddin2 = new Ddin2Sensor(
                    AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                    new SensorData(SensorService.Instance.SensorsCount,
                                   deviceInfo.Name, "Динамограф", _initString));
                ddin2.Notify += SensorService.Instance.SensorDataChangedHandler;
                return ddin2;
            }
            else if(deviceInfo.Name.Contains("DDIM"))
            {
                if (deviceInfo.BluetoothType == BluetoothType.Le)
                {
                    var sensor =  new Ddim2Sensor(
                        AppContainer.Container.Resolve<IBluetoothLeAdapter>
                        (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                        new SensorData(SensorService.Instance.SensorsCount,
                        deviceInfo.Name, "Динамограф", _initString));
                    sensor.Notify += SensorService.Instance.SensorDataChangedHandler;
                    return sensor;
                }
                else if (deviceInfo.BluetoothType == BluetoothType.Classic)
                {
                    var sensor = new Ddim2Sensor(
                        AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                        (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                        new SensorData(SensorService.Instance.SensorsCount,
                        deviceInfo.Name, "Динамограф", _initString));
                    sensor.Notify += SensorService.Instance.SensorDataChangedHandler;
                    return sensor;
                }
            }

            return null;
        }
    }
}
