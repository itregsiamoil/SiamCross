using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{
    public interface ISerialUsbManager
    {
        //List<string> Devices();
        //void ConnectAndSend(byte[] bytesToPrint, int productId, int vendorId);
        void TestWrite();

        void ConnectAndSend();

        void Search();

        void TestAddSensor();

        Task Initialize();
    }
}