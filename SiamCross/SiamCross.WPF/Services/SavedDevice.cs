using Newtonsoft.Json;

namespace SiamCross.WPF.Services
{
    [JsonObject(MemberSerialization.Fields)]
    public class SavedDevice
    {
        public string DeviceName;

        public string DeviceAddress;
    }
}
