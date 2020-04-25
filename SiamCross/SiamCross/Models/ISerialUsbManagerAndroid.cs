using System.Collections.Generic;

namespace SiamCross.Droid.Models
{
    public interface ISerialUsbManager
    {
        List<string> Devices();
        void ConnectAndSend(byte[] bytesToPrint, int productId, int vendorId);
    }
}