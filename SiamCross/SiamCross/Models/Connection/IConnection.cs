using SiamCross.Models.Adapters;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface IConnection
    {
        IPhyInterface PhyInterface { get; }
        Task<bool> Connect();
        void Disconnect();
        void ClearRx();
        void ClearTx();
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
