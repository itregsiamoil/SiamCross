using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2;
using SiamCross.Models.Sensors.Du;
using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M;
using SiamCross.Models.Sensors.Umt;
using System;

namespace SiamCross.Services
{
    public static class SensorFactory
    {
        private static readonly object _locker = new object();

        public static ISensor CreateSensor(ScannedDeviceInfo deviceInfo)
        {
            lock (_locker)
            {
                if (deviceInfo.Name.Contains("DDIN"))
                {
                    var ddin2 = new Ddin2Sensor(
                        AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                        (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                        new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                    ddin2.ScannedDeviceInfo = deviceInfo;

                    return ddin2;
                }
                else if (deviceInfo.Name.Contains("DDIM"))
                {
                    if (deviceInfo.BluetoothType == BluetoothType.Le)
                    {
                        var sensor = new Ddim2Sensor(
                            AppContainer.Container.Resolve<IBluetoothLeAdapter>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                    else if (deviceInfo.BluetoothType == BluetoothType.Classic)
                    {
                        var sensor = new Ddim2Sensor(
                            AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                }
                else if (deviceInfo.Name.Contains("SIDDOSA3M"))
                {
                    if (deviceInfo.BluetoothType == BluetoothType.Le)
                    {
                        var sensor = new SiddosA3MSensor(
                            AppContainer.Container.Resolve<IBluetoothLeAdapter>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                }
                else if (deviceInfo.Name.Contains("DU"))
                {
                    if (deviceInfo.BluetoothType == BluetoothType.Le)
                    {
                        var sensor = new DuSensor(
                            AppContainer.Container.Resolve<IBluetoothLeAdapter>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                    else if (deviceInfo.BluetoothType == BluetoothType.Classic)
                    {
                        var sensor = new DuSensor(
                           AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                           (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                           new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                }
                else if (deviceInfo.Name.Contains("UMT"))
                {
                    switch(deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                            var leBtSensor = new UmtSensor(
                            AppContainer.Container.Resolve<IBluetoothLeAdapter>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.PressureGauge, ""));
                            leBtSensor.ScannedDeviceInfo = deviceInfo;
                            return leBtSensor;
                        case BluetoothType.Classic:
                            var classicBtSensor = new UmtSensor(
                           AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                           (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                           new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.PressureGauge, ""));
                            classicBtSensor.ScannedDeviceInfo = deviceInfo;
                            return classicBtSensor;
                    }
                }

                return null;
            }
        }
    }
}
