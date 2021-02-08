using System;

namespace SiamCross.Models.Scanners
{
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Mac { get; set; }
        public BluetoothType BluetoothType { get; set; }
        public string PrimaryPhy { get; set; }
        public string SecondaryPhy { get; set; }
        public string Rssi { get; set; }
        public string IsLegacy { get; set; }
        public string IsConnectable { get; set; }
        public string TxPower { get; set; }
        public string BondState { get; set; }

        public bool HasSiamServiceUid { get; set; }
        public bool HasUriTag { get; set; }

        public bool Equals(ScannedDeviceInfo other)
        {
            return Name == other.Name;
        }
    }
}
