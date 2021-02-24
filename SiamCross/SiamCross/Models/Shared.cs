using System;
using System.Collections.Generic;

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

    public struct DeviceInfo
    {
        public string Kind;
        public string Number;
        public string Name;
        public uint ProtocolId;
        public Dictionary<string, object> ProtocolData;
        public uint PhyId;
        public Dictionary<string, object> PhyData;
    }
    public struct GeoLocation
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Accuracy;
    }
    public struct PositionInfo
    {
        public int Id;
        public string Field;
        public string Well;
        public string Bush;
        public string Shop;
        public GeoLocation Location;
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

    public struct MeasurementInfo
    {
        public uint Kind;
        public DateTime BeginTimestamp;
        public DateTime EndTimestamp;
        public Dictionary<string, object> Data;
    }

    public struct DistributionInfo
    {
        public DateTime Timestamp;
        public string Destination;
    }
    public struct SurveyInfo
    {
        public int Id;
        public PositionInfo Position;
        public DeviceInfo Device;
        public MeasurementInfo Measure;
        public DistributionInfo MailDistribution;
        public DistributionInfo FileDistribution;
        public string Comment;
    }
}
