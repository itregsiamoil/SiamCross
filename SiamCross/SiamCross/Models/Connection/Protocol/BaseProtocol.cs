using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Connection.Protocol.Siam;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol
{
    public abstract class BaseProtocol : IProtocolConnection
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropChange(PropertyChangedEventArgs arg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        private ConnectionState _ConnState = ConnectionState.Disconnected;
        protected void SetState(ConnectionState state)
        {
            _ConnState = state;
            OnPropChange(new PropertyChangedEventArgs(nameof(State)));
        }

        public ConnectionState State => _ConnState;


        protected IPhyConnection mPhyConn = null;
        public IPhyConnection PhyConnection => mPhyConn;

        private ushort _MaxReqLen = 40;
        public ushort MaxReqLen
        {
            get => _MaxReqLen;
            set
            {
                if (value + 12 + 2 > Pkg.MAX_PKG_SIZE)
                    _MaxReqLen = Pkg.MAX_PKG_SIZE - 12 - 2;
                else
                    _MaxReqLen = value;
                OnPropChange(new PropertyChangedEventArgs(nameof(MaxReqLen)));
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
                    OnPropChange(new PropertyChangedEventArgs(nameof(Address)));
                }
            }
        }

        public BaseProtocol(IPhyConnection base_conn, byte address)
        {
            _Address = address;
            mPhyConn = base_conn;
        }
        public async Task<bool> Connect()
        {
            if (ConnectionState.Connected == mPhyConn.State)
            {
                SetState(ConnectionState.Connected);
                return true;
            }

            bool result = false;
            try
            {
                result = await mPhyConn.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                result = false;
                await Disconnect();
            }
            finally
            {
                if (result)
                {
                    if (ConnectionState.Connected != State)
                        SetState(ConnectionState.Connected);
                }
                else
                {
                    if (ConnectionState.Disconnected != State)
                        SetState(ConnectionState.Disconnected);
                }

            }
            return result;
        }
        public async Task<bool> Disconnect()
        {
            if (ConnectionState.Disconnected == mPhyConn.State)
            {
                SetState(ConnectionState.Disconnected);
                return true;
            }

            bool ret = false;
            SetState(ConnectionState.PendingDisconnect);
            try
            {
                ret = await mPhyConn.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                SetState(ConnectionState.Disconnected);
            }
            return ret;
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
            , Action<float> onStepProgress, CancellationToken cancellationToken);
        public abstract Task<RespResult> TryWriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start
            , Action<float> onStepProgress, CancellationToken cancellationToken);
        public async Task<RespResult> ReadMemAsync(uint addr, uint len
            , byte[] dst, int dst_start
            , Action<float> onStep, CancellationToken ct)
        {
            RespResult ret = await TryReadMemoryAsync(addr, len, dst, dst_start, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }
        public async Task<RespResult> WriteMemAsync(uint addr, uint len
            , byte[] src, int src_start
            , Action<float> onStep, CancellationToken ct)
        {
            RespResult ret = await WriteMemAsync(addr, len, src, src_start, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }


        public virtual Task<RespResult> TryReadAsync(MemStruct var
            , Action<float> onStep, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public virtual Task<RespResult> TryWriteAsync(MemStruct var
            , Action<float> onStep, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public async Task<RespResult> ReadAsync(MemStruct var
            , Action<float> onStep, CancellationToken ct)
        {
            RespResult ret = await TryReadAsync(var, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }
        public async Task<RespResult> WriteAsync(MemStruct var
            , Action<float> onStep, CancellationToken ct)
        {
            RespResult ret = await TryWriteAsync(var, onStep, ct);
            ThrowOnError(ret);
            return ret;
        }


        public Task<RespResult> TryReadAsync(MemVar var
            , Action<float> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return TryReadAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> TryWriteAsync(MemVar var
            , Action<float> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return TryWriteAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> ReadAsync(MemVar var
            , Action<float> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return ReadAsync(tmp_struct, onStep, ct);
        }
        public Task<RespResult> WriteAsync(MemVar var
            , Action<float> onStep, CancellationToken ct)
        {
            MemStruct tmp_struct = new MemStruct(var.Address);
            tmp_struct.Add(var);
            return WriteAsync(tmp_struct, onStep, ct);
        }

    }
}
