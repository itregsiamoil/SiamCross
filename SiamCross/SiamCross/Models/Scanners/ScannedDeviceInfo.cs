using System;

namespace SiamCross.Models.Scanners
{
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public string Name { get; }
        public Guid Id { get; }
        public string Mac { get; }
        public BluetoothType BluetoothType { get; }

        public ScannedDeviceInfo(string name
            , Guid id
            , BluetoothType bluetoothType
            , string mac)
        {
            Name = name;
            Id = id;
            BluetoothType = bluetoothType;
            Mac = mac;
        }

        public bool Equals(ScannedDeviceInfo other)
        {
            return Name == other.Name ? true : false;
        }
    }
}
