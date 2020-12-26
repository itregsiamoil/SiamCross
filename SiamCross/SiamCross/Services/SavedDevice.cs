using Newtonsoft.Json;
using SiamCross.Models;

namespace SiamCross.Services
{
    [JsonObject(MemberSerialization.Fields)]
    public class SavedDevice
    {
        public string DeviceName;

        public string Id;
        public string Mac;

        public BluetoothType BluetoothType;
    }
}
