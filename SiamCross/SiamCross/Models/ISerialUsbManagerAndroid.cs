using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{
    public interface ISerialUsbManager
    {
        void TestWrite();

        void ConnectAndSend();

        void Search();

        void TestAddSensor();

        Task Initialize();

        void Disconnect();

        event Action DataReceived;
        event Action ErrorReceived;
    }
}