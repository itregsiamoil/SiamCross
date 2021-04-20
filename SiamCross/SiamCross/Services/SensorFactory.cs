using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Connection.Protocol.Siam;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Sensors.Dmg.Ddin2;
using SiamCross.Models.Sensors.Du;
using SiamCross.Models.Sensors.Dua;

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
                default: return null;
                case 0x1301:
                case 0x1302:
                case 0x1303:
                case 0x1401:
                case 0x1402:
                case 0x1403:
                    {
                        var model = new DmgSensorModel(connection, deviceInfo.Device);
                        var vm = new Ddin2Sensor(model);
                        return vm;
                    }
                case 0x1101:
                    {
                        IProtocolConnection connection_old = new SiamProtocolConnection(conn);
                        var model = new SensorModel(connection_old, deviceInfo.Device);
                        var vm = new DuSensor(model);
                        return vm;
                    }
                case 0x1201:
                    {
                        var model = new DuaSensorModel(connection, deviceInfo.Device);
                        var vm = new DuaSensor(model);
                        return vm;
                    }

            }
        }
    }
}
