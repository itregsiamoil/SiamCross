using Newtonsoft.Json;
using SiamCross.Models;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [JsonObject(MemberSerialization.Fields)]
    [Preserve(AllMembers = true)]
    public class SavedDevice
    {
        public string DeviceName;

        public string DeviceAddress;

        public BluetoothType BluetoothType;
    }
}
