using System;
using System.Collections.Generic;
using System.Linq;

namespace SiamCross.Models.Scanners
{
    public class ScannedDeviceInfo : IEquatable<ScannedDeviceInfo>
    {
        private static readonly IReadOnlyList<string> _proto_list = ProtocolIndex.Instance.GetNames().ToList();
        public IReadOnlyList<string> ProtocolNames => _proto_list;
        public DeviceInfo Device { get; set; }

        public ScannedDeviceInfo(DeviceInfo dvcInfo)
        {
            Device = dvcInfo;
        }
        public ScannedDeviceInfo()
        {
            Device = new DeviceInfo
            {
                Kind = 0
            };
            Device.ProtocolData["Address"] = "1";
        }

        public ushort GetPrefferedPkgSize()
        {
            if ((uint)BluetoothType.Le == Device.PhyId)
            {
                if (Device.PhyData.TryGetValue("ModemVersion", out object _))
                    return Constants.BTLE_PKG_MAX_SIZE - Constants.BTLE_PKG_HDR_SIZE
                        - Constants.SIAM_PKG_CRC_SIZE - Constants.SIAM_PKG_HDR_SIZE;
                else
                    return Constants.SIAM_PKG_DEFAULT_DATA_SIZE;
            }
            //else if ((uint)BluetoothType.Classic == ScannedDeviceInfo.Device.PhyId)
            {
                return Constants.MAX_PKG_SIZE
                    - Constants.SIAM_PKG_CRC_SIZE - Constants.SIAM_PKG_HDR_SIZE;
            }
        }

        public string ProtocolKindStr
        {
            get
            {
                if (ProtocolIndex.Instance.TryGetName(Device.ProtocolId, out string name))
                    return name;
                return string.Empty;
            }
            set
            {
                if (uint.TryParse(value, out uint idx))
                {
                    if (ProtocolIndex.Instance.TryGetName(idx, out string name))
                    {
                        Device.ProtocolId = idx;
                    }
                    return;
                }
                if (ProtocolIndex.Instance.TryGetId(value, out uint idxx))
                {
                    Device.ProtocolId = idxx;
                }
            }
        }


        public bool Equals(ScannedDeviceInfo other)
        {
            return Mac == other.Mac;
        }

        public string Title => $"{Device.Name} №{Device.Number}";


        public string PhyName
        {
            get
            {
                if (Device.PhyData.TryGetValue("Name", out object objVal))
                    if (objVal is string str)
                        return str;
                return string.Empty;
            }
        }

        public string ProtocolAddress
        {
            get
            {
                string address = string.Empty;
                if (Device.ProtocolData.TryGetValue("Address", out object pi_addr))
                    if (pi_addr is string str)
                        address = str;
                return address;
            }
        }

        public string Mac
        {
            get
            {
                if (Device.PhyData.TryGetValue("Mac", out object objVal))
                    if (objVal is string str)
                        return str;
                return string.Empty;
            }
        }


        public Guid Guid
        {
            get
            {
                if (Device.PhyData.TryGetValue("Guid", out object objVal))
                    if (objVal is string str)
                        if (Guid.TryParse(str, out Guid guid))
                            return guid;
                return new Guid();
            }
        }
        public string Id
        {
            get
            {
                if (Device.PhyData.TryGetValue("Guid", out object objVal))
                    if (objVal is string str)
                        return str;
                return string.Empty;
            }
        }

        public string Description
        {
            get
            {
                string phy = string.Empty;
                if (Device.PhyData.TryGetValue("PrimaryPhy", out object pi_phy))
                    if (pi_phy is string str)
                        phy = str;

                string rssi = string.Empty;
                if (Device.PhyData.TryGetValue("Rssi", out object pi_rssi))
                    if (pi_rssi is string str)
                        rssi = str;

                string mac = string.Empty;
                if (Device.PhyData.TryGetValue("Mac", out object pi_mac))
                    if (pi_mac is string str)
                        mac = str;

                return $" Mac:{mac} Phy:{phy} Rssi:{rssi}";
            }
        }
    }



}
