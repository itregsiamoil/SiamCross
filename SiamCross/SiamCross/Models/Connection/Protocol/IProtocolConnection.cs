using SiamCross.Models.Connection.Phy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol
{
    public enum RespResult
    {
        NormalPkg = 0
      , ErrorCrc = -1
      , ErrorPkg = -2
      , ErrorTimeout = -3
      , ErrorSending = -4
      , ErrorConnection = -5
      , ErrorUnknown = -100
    }
    public interface IProtocolConnection : IConnection
    {

        IPhyConnection PhyConnection { get; }

        Task<byte[]> Exchange(byte[] req);

        ushort MaxReqLen { get; set; }
        byte Address { get; set; }

        Task<RespResult> TryReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);
        Task<RespResult> TryWriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);

    }


}