using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SiamCross.Models.Scanners
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
    public enum ProtocolKind
    {
        Siam = 0
        , Modbus = 1
    }

    public class ProtocolInfo
    {
        public string KindStr
        {
            get => Kind.ToString();
            set
            {
                if (null == value)
                    return;

                if (value.ToString() == ProtocolKind.Modbus.ToString())
                    Kind = ProtocolKind.Modbus;
                else
                    Kind = ProtocolKind.Siam;
            }
        }

        public ProtocolKind Kind { get; set; }

        public byte Address { get; set; }

        public ProtocolInfo(ProtocolKind type = ProtocolKind.Siam, byte addr = 1)
        {
            Kind = type;
            Address = addr;
        }
    }


    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        public ScannedDeviceInfo()
        {
            Protocol = new ProtocolInfo();
        }

        private static readonly List<string> _proto_list = Enum.GetNames(typeof(ProtocolKind)).Select(b => b.SplitCamelCase()).ToList();
        public List<string> ProtocolNames => _proto_list;
        public ProtocolInfo Protocol { get; set; }
        public string Name { get; set; }
        public UInt16 Kind { get; set; }
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
