using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{
    public interface ISerialUsbManager
    {
        void Write(string message);

        Task<bool> Initialize();

        void Disconnect();

        event Action<string> DataReceived;
        event Action ErrorReceived;
    }
}