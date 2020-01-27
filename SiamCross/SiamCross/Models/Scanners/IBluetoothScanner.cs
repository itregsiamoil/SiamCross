using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Scanners
{
    [Preserve(AllMembers = true)]
    public interface IBluetoothScanner
    {
        void Start();

        void Stop();

        event Action<ScannedDeviceInfo> Received;
    }
}
