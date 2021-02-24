using System;
using System.Collections.Generic;
using System.Linq;

namespace SiamCross.Models.Scanners
{
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public ScannedDeviceInfo()
        {
            ProtocolKind = 0;
            ProtocolData["Address"] = "1";
        }
        public string ProtocolKindStr
        {
            get
            {
                if (ProtocolIndex.Instance.TryGetName(ProtocolKind, out string name))
                    return name;
                return string.Empty;
            }
            set
            {
                if (uint.TryParse(value, out uint idx))
                {
                    if (ProtocolIndex.Instance.TryGetName(idx, out string name))
                    {
                        ProtocolKind = idx;
                    }
                    return;
                }
                if (ProtocolIndex.Instance.TryGetId(value, out uint idxx))
                {
                    ProtocolKind = idxx;
                }
            }
        }
        public uint ProtocolKind { get; set; }

        public byte ProtocolAddress
        {
            get
            {
                if (ProtocolData.TryGetValue("Address", out object pi))
                {
                    if (pi is string str)
                    {
                        if (byte.TryParse(str, out byte val))
                            return val;
                    }
                }
                return 1;
            }
            set
            {
                string str_val = "1";
                if (0 < value && 127 > value)
                    str_val = value.ToString();
                ProtocolData["Address"] = str_val;
            }
        }

        public readonly Dictionary<string, object> ProtocolData = new Dictionary<string, object>();
        private static readonly IReadOnlyList<string> _proto_list = ProtocolIndex.Instance.GetNames().ToList();
        public IReadOnlyList<string> ProtocolNames => _proto_list;
        public string Name { get; set; }
        public UInt16 Kind { get; set; }
        public Guid Id { get; set; }
        public string Mac { get; set; }
        public BluetoothType BluetoothType { get; set; }

        public string Description
        {
            get
            {
                string phy = string.Empty;
                if (ProtocolData.TryGetValue("PrimaryPhy", out object pi_phy))
                    if (pi_phy is string str)
                        phy = str;

                string rssi = string.Empty;
                if (ProtocolData.TryGetValue("Rssi", out object pi_rssi))
                    if (pi_rssi is string str)
                        rssi = str;

                return $" Mac:{Mac} Phy:{phy} Rssi:{rssi}";
            }
        }

        public string BondState
        {
            get
            {
                if (ProtocolData.TryGetValue("BondState", out object pi))
                    if (pi is string str)
                        return str;
                return string.Empty;
            }
            set => ProtocolData["BondState"] = value;
        }

        public bool Equals(ScannedDeviceInfo other)
        {
            return Mac == other.Mac;
        }
    }

    public class SiamDeviceInfo
    {
        public SiamDeviceInfo()
        {
            Name = "";
            Num = "";
            Address = 1;
            Kind = 0xFFFF;
        }
        public SiamDeviceInfo(string name, string num, byte address, UInt16 kind)
        {
            Name = name;
            Num = num;
            Address = address;
            Kind = kind;
        }
        public string Name { get; set; }
        public string Num { get; set; }
        public byte Address { get; set; }
        public UInt16 Kind { get; set; }
    }

}
