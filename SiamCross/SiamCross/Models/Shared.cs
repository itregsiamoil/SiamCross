using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Xamarin.Forms.Internals;

namespace SiamCross.Models
{
    public static class PhyIndex
    {
        public static readonly KeyKeyCollection<string> Instance = new KeyKeyCollection<string>();
        static PhyIndex()
        {
            Instance.Add(0, "Bluetooth 2");
            Instance.Add(1, "Bluetooth Le");
            Instance.Add(2, "UART");
        }
    }
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
            Instance.Add(2, Resource.Manometr);
        }
    }
    public static class DeviceIndex
    {
        public static readonly KeyKeyCollection<string> Instance = new KeyKeyCollection<string>();
        static DeviceIndex()
        {
            Instance.Add(0x1101, $"{Resource.Levelmeter} {Resource.Manual} v.01");
            Instance.Add(0x1201, $"{Resource.Levelmeter} {Resource.Automatic} v.01");
            Instance.Add(0x1301, $"{Resource.Dynamograph} {Resource.Overhead} v.01");
            Instance.Add(0x1302, $"{Resource.Dynamograph} {Resource.Overhead} v.02");
            Instance.Add(0x1303, $"{Resource.Dynamograph} {Resource.Overhead} v.03");
            Instance.Add(0x1401, $"{Resource.Dynamograph} {Resource.Intertraversal} v.01");
            Instance.Add(0x1402, $"{Resource.Dynamograph} {Resource.Intertraversal} v.02");
            Instance.Add(0x1403, $"{Resource.Dynamograph} {Resource.Intertraversal} v.03");
            Instance.Add(0x1700, $"{Resource.Manometr}+{Resource.Thermometer} v.00");
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
        public uint Number;
        public string Name;
        public uint ProtocolId;
        [XmlIgnore]
        public Dictionary<string, object> ProtocolData = new Dictionary<string, object>();
        public uint PhyId;
        [XmlIgnore]
        public Dictionary<string, object> PhyData = new Dictionary<string, object>();
        [XmlIgnore]
        public Dictionary<string, object> DeviceData = new Dictionary<string, object>();

        public bool TryGetData<T>(string name, out T value)
        {
            if (DeviceData.TryGetValue(name, out object objVal))
                if (objVal is T val)
                {
                    value = val;
                    return true;
                }
            value = default;
            return false;
        }


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
            Number = 0;
            Name = string.Empty;
            ProtocolId = 0;
            PhyId = 0;
        }

        public object Clone()
        {
            var ret = new DeviceInfo
            {
                Kind = Kind,
                Number = Number,
                ProtocolId = ProtocolId,
                PhyId = PhyId,
                PhyDataArray = PhyDataArray,
                ProtocolDataArray = ProtocolDataArray
            };
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
    public class Position
    {
        public uint Field;
        public string Well;
        public string Bush;
        public uint Shop;
        //public GeoLocation Location;
        public Position(
              uint field = 0, string well = "0"
            , string bush = "0", uint shop = 0)
        {
            Field = field;
            Well = well;
            Bush = bush;
            Shop = shop;
            //Location = new GeoLocation();
        }
    }

    public enum AttributeType
    {
        Int = 1,
        Float = 5,
        String = 2,
        Blob = 3
    };

    [Preserve(AllMembers = true)]
    public class AttributeItem
    {
        public static int Type2Int(AttributeType t)
        {
            switch (t)
            {
                case AttributeType.Int: return 1;
                case AttributeType.Float: return 5;
                case AttributeType.String: return 2;
                case AttributeType.Blob: return 3;
                default: break;
            }
            new Exception("Wrong Attribute type");
            return 0;
        }

        public uint Id { get; set; }
        public string Title { get; set; }
        public int TypeId { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class MeasurementInfo
    {
        public uint Kind;
        public DateTime BeginTimestamp;
        public DateTime EndTimestamp;
        public string Comment;

        public Dictionary<AttributeItem, long> DataInt = new Dictionary<AttributeItem, long>();
        public Dictionary<AttributeItem, double> DataFloat = new Dictionary<AttributeItem, double>();
        public Dictionary<AttributeItem, string> DataString = new Dictionary<AttributeItem, string>();
        public Dictionary<AttributeItem, string> DataBlob = new Dictionary<AttributeItem, string>();

        public bool TryGet(string title, out long value)
        {
            if (Repo.AttrDir.ByTitle.TryGetValue(title, out AttributeItem attr))
                return DataInt.TryGetValue(attr, out value);
            value = 0;
            return false;
        }
        public bool TryGet(string title, out double value)
        {
            if (Repo.AttrDir.ByTitle.TryGetValue(title, out AttributeItem attr))
                return DataFloat.TryGetValue(attr, out value);
            value = 0f;
            return false;
        }
        public bool TryGet(string title, out string value)
        {
            if (Repo.AttrDir.ByTitle.TryGetValue(title, out AttributeItem attr))
                return DataString.TryGetValue(attr, out value);
            value = string.Empty;
            return false;
        }
        public bool TryGetBlob(string title, out string value)
        {
            if (Repo.AttrDir.ByTitle.TryGetValue(title, out AttributeItem attr))
                return DataBlob.TryGetValue(attr, out value);
            value = string.Empty;
            return false;
        }
        public void Set(string title, long value)
        {
            var attr = Repo.AttrDir.ByTitle[title];
            DataInt[attr] = value;
        }
        public void Set(string title, double value)
        {
            var attr = Repo.AttrDir.ByTitle[title];
            DataFloat[attr] = value;
        }
        public void Set(string title, string value)
        {
            var attr = Repo.AttrDir.ByTitle[title];
            DataString[attr] = value;
        }
        public void SetBlob(string title, string value)
        {
            var attr = Repo.AttrDir.ByTitle[title];
            DataBlob[attr] = value;
        }
    }

    [Serializable]
    public class CommonInfo
    {
        [XmlIgnore]
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        [XmlArray("Info")]
        [XmlArrayItem("Item")]
        public KeyVal[] DataArray
        {
            get => KeyVal.ToArray(Data);
            set => KeyVal.FromArray(value, Data);
        }
    }


    [Serializable]
    public class DistributionInfo
    {
        public DateTime Timestamp;
        public string Destination;
    }
    [Serializable]
    public class MeasureData
    {
        public long Id = 0;
        public Position Position;
        public DeviceInfo Device;
        public CommonInfo Info;
        public MeasurementInfo Measure;
        public DistributionInfo MailDistribution;
        public DistributionInfo FileDistribution;

        public MeasureData(Position pos
            , DeviceInfo dev
            , CommonInfo info
            , MeasurementInfo measure
            , DistributionInfo mail = null
            , DistributionInfo file = null)
        {
            Position = pos;
            Device = dev;
            Info = info;
            Measure = measure;
            MailDistribution = mail ?? new DistributionInfo();
            FileDistribution = file ?? new DistributionInfo();
        }
    }
}
