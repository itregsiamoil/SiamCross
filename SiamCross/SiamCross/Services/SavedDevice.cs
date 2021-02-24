using Newtonsoft.Json;
using SiamCross.Models;
using System;

namespace SiamCross.Services
{
    [JsonObject(MemberSerialization.Fields)]
    public class SavedDevice
    {
        public string DeviceName;
        public string Id;
        public string Mac;
        public BluetoothType BluetoothType;
        public UInt16 Kind;
        public uint ProtocolId;
        public byte ProtocolAddress;

    }
}
