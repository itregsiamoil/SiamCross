using SiamCross.Models.Adapters;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Phy
{
    public interface IPhyConnection : IConnection
    {
        int Mtu { get; }
        int Rssi { get; }
        IPhyInterface PhyInterface { get; }
        void UpdateRssi();
        void ClearRx();
        void ClearTx();
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
