using SiamCross.Models.Adapters;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IPhyConnection
    {
        void UpdateRssi();
        int Rssi { get; }
        IPhyInterface PhyInterface { get; }
        Task<bool> Connect();
        Task<bool> Disconnect();
        void ClearRx();
        void ClearTx();
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
