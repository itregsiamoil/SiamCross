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
        int RequestCount { get; set; }
        int ResponseCount { get; set; }
        IPhyConnection PhyConnection { get; }

        Task<byte[]> Exchange(byte[] req);

        int AdditioonalTimeout { get; set; }
        ushort MaxReqLen { get; set; }
        byte Address { get; set; }

        int Retry { get; set; }

        Task<RespResult> TryReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default);
        Task<RespResult> TryWriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default);
        Task<RespResult> ReadMemAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default);
        Task<RespResult> WriteMemAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default);


        Task<RespResult> TryReadAsync(MemStruct var
            , Action<uint> onStep = null, CancellationToken ct = default);
        Task<RespResult> TryWriteAsync(MemStruct var
            , Action<uint> onStep = null, CancellationToken ct = default);
        Task<RespResult> ReadAsync(MemStruct var
            , Action<uint> onStep = null, CancellationToken ct = default);
        Task<RespResult> WriteAsync(MemStruct var
            , Action<uint> onStep = null, CancellationToken ct = default);


        Task<RespResult> TryReadAsync(MemVar var
            , Action<uint> onStep = null, CancellationToken ct = default);
        Task<RespResult> TryWriteAsync(MemVar var
            , Action<uint> onStep = null, CancellationToken ct = default);

        Task<RespResult> ReadAsync(MemVar var
            , Action<uint> onStep = null, CancellationToken ct = default);
        Task<RespResult> WriteAsync(MemVar var
            , Action<uint> onStep = null, CancellationToken ct = default);

    }


}