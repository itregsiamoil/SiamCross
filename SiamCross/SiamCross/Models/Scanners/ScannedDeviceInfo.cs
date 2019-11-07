using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Scanners
{
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public string Name { get; }
        public object BluetoothArgs { get; }

        public ScannedDeviceInfo(string name, object bluetoothArgs)
        {
            Name = name;
            BluetoothArgs = bluetoothArgs;
        }

        public bool Equals(ScannedDeviceInfo other)
        {
            return this.Name == other.Name ? true : false;
        }
    }
}
