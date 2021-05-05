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
            Instance.Add(2, Resource.Manometr);
        }
    }
    public static class DeviceIndex
    {
        public static readonly KeyKeyCollection<string> Instance = new KeyKeyCollection<string>();
        static DeviceIndex()
        {
            Instance.Add(0x1101, Resource.LevelGaugeSensorType + " 0x1101");
            Instance.Add(0x1201, Resource.LevelGaugeSensorType + " 0x1201");
            Instance.Add(0x1301, Resource.DynamographSensorType + " 0x1301");
            Instance.Add(0x1302, Resource.DynamographSensorType + " 0x1302");
            Instance.Add(0x1303, Resource.DynamographSensorType + " 0x1303");
            Instance.Add(0x1401, Resource.DynamographSensorType + " 0x1401");
            Instance.Add(0x1402, Resource.DynamographSensorType + " 0x1402");
            Instance.Add(0x1403, Resource.DynamographSensorType + " 0x1403");
            Instance.Add(0x1700, Resource.Manometr + " 0x1700");
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
    [Serializable]
    public class MeasurementInfo
    {
        public uint Kind;
        public DateTime BeginTimestamp;
        public DateTime EndTimestamp;
        public string Comment;

        public Dictionary<string, long> DataInt = new Dictionary<string, long>();
        public Dictionary<string, double> DataFloat = new Dictionary<string, double>();
        public Dictionary<string, string> DataString = new Dictionary<string, string>();
        public Dictionary<string, string> DataBlob = new Dictionary<string, string>();

        [XmlIgnore]
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        [XmlArray("Data")]
        [XmlArrayItem("Item")]
        public KeyVal[] DataArray
        {
            get => KeyVal.ToArray(Data);
            set => KeyVal.FromArray(value, Data);
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
        public Position Position = new Position();
        public DeviceInfo Device = new DeviceInfo();
        public CommonInfo Info = new CommonInfo();
        public MeasurementInfo Measure = new MeasurementInfo();
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
