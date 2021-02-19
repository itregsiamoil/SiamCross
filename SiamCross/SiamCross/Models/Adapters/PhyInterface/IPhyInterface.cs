using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Scanners;

namespace SiamCross.Models.Adapters
{
    public interface IPhyInterface
    {
        string Name { get; }
        void Disable();
        void Enable();

        IPhyConnection MakeConnection(ScannedDeviceInfo deviceInfo);

        IBluetoothScanner GetScanner();
        bool IsEnbaled { get; }
    }
}
