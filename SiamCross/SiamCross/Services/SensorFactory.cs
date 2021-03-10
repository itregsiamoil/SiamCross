using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
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

            switch ((BluetoothType)deviceInfo.Device.PhyId)
            {
                default: break;
                case BluetoothType.Le:
                    phy_interface = FactoryBtLe.GetCurent(); break;
                case BluetoothType.Classic:
                    phy_interface = FactoryBt2.GetCurent(); break;
            }
            if (null == phy_interface)
                return null;
            IPhyConnection conn = phy_interface.MakeConnection(deviceInfo);
            IProtocolConnection connection = new SiamConnection(conn);

            switch (deviceInfo.Device.Kind)
            {
                case 0x1301:
                case 0x1302:
                case 0x1303:
                case 0x1401:
                case 0x1402:
                case 0x1403:
                    {
                        Ddin2Sensor sensor = new Ddin2Sensor(connection, deviceInfo);
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                case 0x1101:
                    {
                        IProtocolConnection connection_old = new SiamProtocolConnection(conn);
                        DuSensor sensor = new DuSensor(connection_old, deviceInfo);
                        sensor.ScannedDeviceInfo = deviceInfo;
                        return sensor;
                    }
                default:
                    return null; ;
            }
        }
    }
}
