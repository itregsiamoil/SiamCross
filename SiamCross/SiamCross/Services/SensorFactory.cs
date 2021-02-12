using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Connection.Protocol.Siam;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg.Ddin2;
using SiamCross.Models.Sensors.Du;
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
                    phy_interface = Models.Adapters.PhyInterface.Bt2.Factory.GetCurent(); break;
            }
            if (null == phy_interface)
                return null;
            IPhyConnection conn = phy_interface.MakeConnection(deviceInfo);
            IProtocolConnection connection = new SiamProtocolConnection(conn);
            if (null == connection)
                return null;
            //lock (_locker)
            {
                if (deviceInfo.Name.Contains("DDIN"))
                {
                    SensorData sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    Ddin2Sensor sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("DDIM"))
                {
                    SensorData sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    Ddin2Sensor sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("SIDDOSA3M"))
                {
                    SensorData sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.DynamographSensorType, "");
                    Ddin2Sensor sensor = new Ddin2Sensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }
                else if (deviceInfo.Name.Contains("DU"))
                {
                    SensorData sens_data = new SensorData(Guid.NewGuid(), deviceInfo.Name, Resource.LevelGaugeSensorType, "");
                    DuSensor sensor = new DuSensor(connection, sens_data);
                    sensor.ScannedDeviceInfo = deviceInfo;
                    return sensor;
                }

                return null;
            }
        }
    }
}
