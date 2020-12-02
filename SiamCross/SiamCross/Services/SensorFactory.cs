using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg.Ddim2;
using SiamCross.Models.Sensors.Dmg.Ddin2;
using SiamCross.Models.Sensors.Dmg.SiddosA3M;
using SiamCross.Models.Sensors.Du;
using SiamCross.Models.Sensors.Umt;
using System;

namespace SiamCross.Services
{
    public static class SensorFactory
    {
        //private static readonly object _locker = new object();

        public static ISensor CreateSensor(ScannedDeviceInfo deviceInfo)
        {
            IPhyInterface phy_interface = null;

            switch (deviceInfo.BluetoothType)
            {
                default: break;
                case BluetoothType.Le:
                    phy_interface = BtLeInterface.Factory.GetCurent(); break;
                case BluetoothType.Classic:
                    phy_interface = Bt2Interface.Factory.GetCurent(); break;
            }
            if (null == phy_interface)
                return null;
            var connection = phy_interface.MakeConnection(deviceInfo);
            if (null == connection)
                return null;
            //lock (_locker)
            {
                if (deviceInfo.Name.Contains("DDIN"))
                {
                    var sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    var sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("DDIM"))
                {
                    var sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    var sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("SIDDOSA3M"))
                {
                    var sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    var sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("DU"))
                {
                    switch (deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                        {
                            var sensor = new DuSensor(
                                AppContainer.Container.Resolve<IConnectionBtLe>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                        case BluetoothType.Classic:
                        {
                            var sensor = new DuSensor(
                                AppContainer.Container.Resolve<IConnectionBt2>
                                    (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, ""));
                            sensor.ScannedDeviceInfo = deviceInfo;
                            return sensor;
                        }
                    }
                }
                else if (deviceInfo.Name.Contains("UMT") || deviceInfo.Name.Contains("DMT"))
                {
                    switch(deviceInfo.BluetoothType)
                    {
                        case BluetoothType.Le:
                            var leBtSensor = new UmtSensor(
                            AppContainer.Container.Resolve<IConnectionBtLe>
                            (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                            new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.PressureGauge, ""));
                            leBtSensor.ScannedDeviceInfo = deviceInfo;
                            return leBtSensor;
                        case BluetoothType.Classic:
                            var classicBtSensor = new UmtSensor(
                           AppContainer.Container.Resolve<IConnectionBt2>
                           (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                           new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.PressureGauge, ""));
                            classicBtSensor.ScannedDeviceInfo = deviceInfo;
                            return classicBtSensor;
                        case BluetoothType.UsbCustom5:
                            {
                                var sensor = new UmtSensor(
                                    AppContainer.Container.Resolve<IBluetooth5CustomAdapter>
                                        (new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo)),
                                    new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.PressureGauge, ""));
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
