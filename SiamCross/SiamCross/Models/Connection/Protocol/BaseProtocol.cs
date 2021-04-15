using SiamCross.Models.Connection.Phy;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol
{
    public abstract class BaseProtocol : ViewModels.BaseVM, IProtocolConnection
    {
        void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            if (sender.Equals(mPhyConn) && e.PropertyName == nameof(Connection.IConnection.State))
                ChangeNotify(nameof(State));
        }
        public ConnectionState State => mPhyConn.State;


        protected IPhyConnection mPhyConn = null;
        public IPhyConnection PhyConnection => mPhyConn;

        private ushort _MaxReqLen = 40;
        public ushort MaxReqLen
        {
            get => _MaxReqLen;
            set
            {
                var new_val = value + Constants.SIAM_PKG_CRC_SIZE + Constants.SIAM_PKG_HDR_SIZE;

                if (new_val > Constants.MAX_PKG_SIZE)
                    _MaxReqLen = Constants.MAX_PKG_SIZE
                        - Constants.SIAM_PKG_CRC_SIZE - Constants.SIAM_PKG_HDR_SIZE;
                else
                    _MaxReqLen = value;
                ChangeNotify();
            }
        }

        private byte _Address = 1;
        public virtual byte Address
        {
            get => _Address;
            set
            {
                if (0 < value && 128 > value)
                {
                    _Address = value;
                    ChangeNotify();
                }
            }
        }

        private int _Retry = 3;
        public int Retry { get => _Retry; set => _Retry = value; }

        protected int mAdditioonTime = 2000;
        public int AdditioonalTimeout
        {
            get => mAdditioonTime;
            set => mAdditioonTime = value;
        }
        public BaseProtocol(IPhyConnection base_conn, byte address)
        {
            _Address = address;
            mPhyConn = base_conn;
            mPhyConn.PropertyChanged += OnConnectionChange;
        }
        public virtual Task<bool> Connect(CancellationToken ct)
        {
            return mPhyConn.Connect(ct);
        }
        public virtual Task<bool> Disconnect()
        {
            return mPhyConn.Disconnect();
        }

        private void ThrowOnError(RespResult ret)
        {
            switch (ret)
            {
                case RespResult.NormalPkg: break;
                case RespResult.ErrorCrc: throw new IOCrcException();
                case RespResult.ErrorPkg: throw new IOErrPkgException();
                case RespResult.ErrorTimeout: throw new IOTimeoutException();
                case RespResult.ErrorSending:
                case RespResult.ErrorConnection:
                case RespResult.ErrorUnknown:
                default: throw new ProtocolException();
            }
        }

        public abstract Task<byte[]> Exchange(byte[] req);

        public abstract Task<RespResult> TryReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start
            , Action<uint> onStepProgress, CancellationToken cancellationToken);
        public abstract Task<RespResult> TryWriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start
            , Action<uint> onStepProgress, CancellationToken cancellationToken);
        public async Task<RespResult> ReadMemAsync(uint addr, uint len
            , byte[] dst, int dst_start
            , Action<uint> onStep, CancellationToken ct)
        {
            RespResult ret = await TryReadMemoryAsync(addr, len, dst, dst_start, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }
        public async Task<RespResult> WriteMemAsync(uint addr, uint len
            , byte[] src, int src_start
            , Action<uint> onStep, CancellationToken ct)
        {
            RespResult ret = await WriteMemAsync(addr, len, src, src_start, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }


        public virtual Task<RespResult> TryReadAsync(MemStruct var
            , Action<uint> onStep, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public virtual Task<RespResult> TryWriteAsync(MemStruct var
            , Action<uint> onStep, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public async Task<RespResult> ReadAsync(MemStruct var
            , Action<uint> onStep, CancellationToken ct)
        {
            RespResult ret = await TryReadAsync(var, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }
        public async Task<RespResult> WriteAsync(MemStruct var
            , Action<uint> onStep, CancellationToken ct)
        {
            RespResult ret = await TryWriteAsync(var, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }


        public Task<RespResult> TryReadAsync(MemVar var
            , Action<uint> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return TryReadAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> TryWriteAsync(MemVar var
            , Action<uint> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return TryWriteAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> ReadAsync(MemVar var
            , Action<uint> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return ReadAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> WriteAsync(MemVar var
            , Action<uint> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return WriteAsync(tmp_struct, onStep, ct);
        }

        public virtual void Dispose()
        {
            mPhyConn.PropertyChanged -= OnConnectionChange;
        }
    }
}
