using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Adapters
{
    public interface IPhyInterface
    {
        string Name { get; }
        void Disable();
        void Enable();

        IProtocolConnection MakeConnection(ScannedDeviceInfo deviceInfo);
        bool IsEnbaled { get; }
    }
}
