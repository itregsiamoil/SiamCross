using SiamCross.Models.Connection.Phy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol
{
    public interface IProtocolConnection : IConnection
    {
        IPhyConnection PhyConnection { get; }

        Task<byte[]> Exchange(byte[] req);

        ushort MaxReqLen { get; set; }
        byte Address { get; set; }

        Task<bool> ReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);
        Task<bool> WriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);

    }


}