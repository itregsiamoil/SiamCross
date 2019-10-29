using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models
{
    public interface IBluetoothScaner
    {
        void Start();
        void Stop();
        event Action<string, object> Received;
    }
}
