using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SiamCross.Models
{
    public static class ProtocolIndex
    {
        public static readonly KeyKeyCollection<string> Instance = new KeyKeyCollection<string>();
        static ProtocolIndex()
        {
            Instance.Add(0, "Siam");
            Instance.Add(1, "Modbus");
        }
    }
    public static class MeasurementIndex
    {
        public static readonly KeyKeyCollection<string> Instance = new KeyKeyCollection<string>();
        static MeasurementIndex()
        {
            Instance.Add(0, Resource.Dynamogram);
            Instance.Add(1, Resource.Echogram);
        }
    }

    [Serializable]
    public class KeyVal
    {
        public KeyVal()
        { }
        public KeyVal(string key, string val)
        {
            Key = key;
            Val = val;
        }
        public string Key;
        public string Val;
        public static void FromArray(KeyVal[] arr, Dictionary<string, object> dict)
        {
            dict.Clear();
            foreach (var item in arr)
                dict.Add(item.Key, item.Val);
        }
        public static KeyVal[] ToArray(Dictionary<string, object> dict)
        {
            KeyVal[] ret = new KeyVal[dict.Count];
            int i = 0;
            foreach (var item in dict)
            {
                if (item.Value is string str)
                    ret[i] = new KeyVal(item.Key, str);
                else
                    ret[i] = new KeyVal(item.Key, string.Empty);
                ++i;
            }
            return ret;
        }
    }

    [Serializable]
    public class DeviceInfo : ICloneable
    {
        public uint Kind;
        public string Number;
        public string Name;
        public uint ProtocolId;
        [XmlIgnore]
        public Dictionary<string, object> ProtocolData;
        public uint PhyId;
        [XmlIgnore]
        public Dictionary<string, object> PhyData;

        [XmlArray("ProtocolData")]
        [XmlArrayItem("Item")]
        public KeyVal[] ProtocolDataArray
        {
            get => KeyVal.ToArray(ProtocolData);
            set => KeyVal.FromArray(value, ProtocolData);
        }
        [XmlArray("PhyData")]
        [XmlArrayItem("Item")]
        public KeyVal[] PhyDataArray
        {
            get => KeyVal.ToArray(PhyData);
            set => KeyVal.FromArray(value, PhyData);
        }

        public DeviceInfo()
        {
            Kind = 0;
            Number = string.Empty;
            Name = string.Empty;
            ProtocolId = 0;
            PhyId = 0;
            ProtocolData = new Dictionary<string, object>();
            PhyData = new Dictionary<string, object>();
        }

        public object Clone()
        {
            var ret = new DeviceInfo();
            ret.Kind = Kind;
            ret.Number = Number;
            ret.ProtocolId = ProtocolId;
            ret.PhyId = PhyId;
            ret.PhyDataArray = PhyDataArray;
            ret.ProtocolDataArray = ProtocolDataArray;
            return ret;
        }
    }
    [Serializable]
    public class GeoLocation
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Accuracy;
    }
    [Serializable]
    public class PositionInfo
    {
        public int Id;
        public string Field;
        public string Well;
        public string Bush;
        public string Shop;
        public GeoLocation Location = new GeoLocation();
    }
    [Serializable]
    public class MeasurementInfo
    {
        public uint Kind;
        public DateTime BeginTimestamp;
        public DateTime EndTimestamp;
        public string Comment;
        public Dictionary<string, object> Data = new Dictionary<string, object>();

    }
    [Serializable]
    public class DistributionInfo
    {
        public DateTime Timestamp;
        public string Destination;
    }
    [Serializable]
    public class SurveyInfo
    {
        public int Id = 0;
        public PositionInfo Position = new PositionInfo();
        public DeviceInfo Device = new DeviceInfo();
        public MeasurementInfo Measure = new MeasurementInfo();
        public DistributionInfo MailDistribution = new DistributionInfo();
        public DistributionInfo FileDistribution = new DistributionInfo();

    }
}
