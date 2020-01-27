using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Scanners
{
    [Preserve(AllMembers = true)]
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public string Name { get; }
        public object BluetoothArgs { get; }

        public BluetoothType BluetoothType { get; }

        public ScannedDeviceInfo(string name, object bluetoothArgs, BluetoothType bluetoothType)
        {
            Name = name;
            BluetoothArgs = bluetoothArgs;
            BluetoothType = bluetoothType;
        }

        public bool Equals(ScannedDeviceInfo other)
        {
            return this.Name == other.Name ? true : false;
        }
    }
}
