using SiamCross.Models.Scanners;

namespace SiamCross.Models.Adapters
{
    public interface IPhyInterface
    {
        string Name { get; }
        void Disable();
        void Enable();

        IPhyConnection MakeConnection(ScannedDeviceInfo deviceInfo);
        bool IsEnbaled { get; }
    }
}
