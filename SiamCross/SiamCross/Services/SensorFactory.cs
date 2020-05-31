using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddin2;
using SiamCross.Models.Sensors.Du;
using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M;
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
                    switch (deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                        {
                            var sensor = new Ddim2Sensor(
                                AppContainer.Container.Resolve<IBluetoothLeAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                        case BluetoothType.Classic:
                        {
                            var sensor = new Ddim2Sensor(
                                AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                        case BluetoothType.UsbCustom5:
                        {
                            var sensor = new Ddim2Sensor(
                                AppContainer.Container.Resolve<IBluetooth5CustomAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                    }
                }
                else if (deviceInfo.Name.Contains("SIDDOSA3M"))
                {
                    switch (deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                        {
                            var sensor = new SiddosA3MSensor(
                                AppContainer.Container.Resolve<IBluetoothLeAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                    }
                }
                else if (deviceInfo.Name.Contains("DU"))
                {
                    switch (deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                        {
                            var sensor = new DuSensor(
                                AppContainer.Container.Resolve<IBluetoothLeAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                        case BluetoothType.Classic:
                        {
                            var sensor = new DuSensor(
                                AppContainer.Container.Resolve<IBluetoothClassicAdapter>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                    }
                }

                return null;
            }
        }
    }
}
