using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{
    public interface ISerialUsbManager
    {
        void Write(string message);

        Task Initialize();

        void Disconnect();

        event Action DataReceived;
        event Action ErrorReceived;
    }
}