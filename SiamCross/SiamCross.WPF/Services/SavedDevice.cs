using Newtonsoft.Json;
using SiamCross.Models;

namespace SiamCross.WPF.Services
{
    [JsonObject(MemberSerialization.Fields)]
    public class SavedDevice
    {
        public string DeviceName;

        public string DeviceAddress;

        public BluetoothType BluetoothType;
    }
}
