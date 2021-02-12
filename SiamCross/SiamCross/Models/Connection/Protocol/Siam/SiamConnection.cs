#define DEBUG_UNIT
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol.Siam
{
    public class SiamConnection : BaseProtocol
    {
        

        public SiamConnection(IPhyConnection base_conn, byte address=1)
            : base(base_conn, address)
        {
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly byte[] _RxBuf = new byte[Pkg.MAX_PKG_SIZE * 2];
        private int _BeginRxBuf = 0;
        private int _EndRxBuf = 0;
        private readonly byte[] _TxBuf = new byte[Pkg.MAX_PKG_SIZE];
        private int _EndTxBuf = 0;

        private static readonly int _Retry = 3;
        private static readonly int _ResponseRetry = Pkg.MAX_PKG_SIZE;

        private static readonly int mAdditioonTime = 500;
        private static readonly int mMinSpeed = 9600; ///bit per second
        private static readonly float multipler = 1000.0f / (mMinSpeed / (8 + 1 + 1));
        private static int GetTime(int bytes)
        {
            // 1000000usec
            // mMinSpeed/(8+1+1) - byte per second
            //one byte = 1000000 / (9600 / (8 + 1 + 1)) / 1000 = 1,04166msec
            //int timeout = 1000000 / (mMinSpeed / (8 + 1 + 1)) * bytes / 1000;
            int timeout = (int)(bytes * multipler + 0.5);
            return 1 > timeout ? 1 : timeout;
        }
        private static int GetRequestTimeout(int len)
        {
            return GetTime(len) + mAdditioonTime;
        }
        private static int GetResponseTimeout(byte[] rq)
        {
            int timeout = 0;
            if (null == rq)
                return timeout;
            switch (rq[3])
            {
                default: break;
                case 0x01:
                    ushort data_len = BitConverter.ToUInt16(rq, 8);
                    timeout = GetTime(rq.Length + data_len + 2);
                    break;
                case 0x02:
                    timeout = GetTime(rq.Length);
                    break;
            }
            return timeout;
        }

        private readonly Stopwatch _PerfCounter = new Stopwatch();

        private void CheckEmptySpace()
        {
            int empty_space = _RxBuf.Length - _EndRxBuf;
            if (empty_space < Pkg.MAX_PKG_SIZE)
            {
                Span<byte> dst_buf = _RxBuf;
                Span<byte> src_buf = _RxBuf.AsSpan().Slice(_BeginRxBuf, _EndRxBuf);
                src_buf.CopyTo(dst_buf);
            }
        }
        private async Task<bool> RequestAsync()
        {
            int write_timeout = GetRequestTimeout(_EndTxBuf);
            CancellationTokenSource ctSrc = new CancellationTokenSource(write_timeout);
            bool sent_ok = false;
            for (int i = 0; i < _Retry && !sent_ok; ++i)
            {
                ctSrc.Token.ThrowIfCancellationRequested();
                int sent = await mPhyConn.WriteAsync(_TxBuf, 0, _EndTxBuf, ctSrc.Token);
                if (_EndTxBuf == sent)
                    sent_ok = true;
                DebugLog.WriteLine("Sent " + _EndTxBuf.ToString()
                    + " elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString()
                    + ": [" + BitConverter.ToString(_TxBuf, 0, _EndTxBuf) + "]\n");
            }
            //if (!sent)
            //    ConnectFailed?.Invoke();
            return sent_ok;
        }
        private enum RespResult
        {
            NormalPkg = 0
          , ErrorCrc = -1
          , ErrorPkg = -2
          , ErrorTimeout = -3
          , ErrorSending = -4
          , ErrorConnection = -5
          , ErrorUnknown = -100
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// 0 < -   some received OK, starting pos in mBeginRxBuf
        /// -1  -   CRC error
        /// -2  -   errorPkg received OK, starting pos mBeginRxBuf
        /// -3  -   Timeout error
        /// -4  -   Sending error
        /// </returns>
        private async Task<RespResult> ResponseAsync()
        {
            int pf_delay = GetResponseTimeout(_TxBuf);
            int read_timeout = GetResponseTimeout(_TxBuf) + mAdditioonTime;
            CancellationTokenSource ctSrc = new CancellationTokenSource(read_timeout);

            _BeginRxBuf = 0;
            _EndRxBuf = 0;
            int need = Pkg.MIN_PKG_SIZE;
            bool is_ok = false;
            try
            {
                for (int i = 0; i < _ResponseRetry && !is_ok; ++i)
                {
                    CheckEmptySpace();
                    int readed = await mPhyConn.ReadAsync(_RxBuf, _EndRxBuf, _RxBuf.Length - _EndRxBuf, ctSrc.Token);
                    _EndRxBuf += readed;
                    DebugLog.WriteLine($"Appended bytes={readed} elapsed={_PerfCounter.ElapsedMilliseconds}/{read_timeout}");
                    Pkg.Extract(_TxBuf, _RxBuf, ref _BeginRxBuf, _EndRxBuf, ref need);
                    if (-2 > need)
                        return RespResult.ErrorUnknown;
                    switch (need)
                    {
                        default:
                            break;
                        case 0:
                            DebugLog.WriteLine($"GET response "
                                + $" expected={pf_delay}"
                                + $" elapsed={_PerfCounter.ElapsedMilliseconds}/{read_timeout}"
                                + ": [" + BitConverter.ToString(_RxBuf, 0, _EndRxBuf) + "]\n");
                            return RespResult.NormalPkg;
                        case -1: return RespResult.ErrorCrc;
                        case -2: return RespResult.ErrorPkg;
                    }
                }//for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            }
            catch (OperationCanceledException)
            {
                DebugLog.WriteLine("Timeout response"
                    + " elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString()
                    + " / " + read_timeout.ToString()
                    + ": [" + BitConverter.ToString(_RxBuf, 0, _EndRxBuf) + "]\n");
            }
            finally
            {
                ctSrc.Dispose();
            }
            return RespResult.ErrorTimeout;

        }
        private async Task<RespResult> SingleExchangeAsync()
        {
            try
            {
                mPhyConn.ClearRx();
                mPhyConn.ClearTx();
                bool sent = await RequestAsync();
                if (!sent)
                    return RespResult.ErrorSending;
                return await ResponseAsync();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("Unknown EXCEPTION in response"
                    + " elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString()
                    + ": [" + BitConverter.ToString(_RxBuf, 0, _EndRxBuf) + "]\n"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return RespResult.ErrorUnknown;
        }
        private static bool NeedRetry(RespResult result)
        {
            switch (result)
            {
                default:
                case RespResult.ErrorConnection:
                case RespResult.ErrorPkg: 
                case RespResult.NormalPkg: return false;

                case RespResult.ErrorUnknown:
                case RespResult.ErrorSending:
                case RespResult.ErrorTimeout:
                case RespResult.ErrorCrc: return true;
            }
        }
        private async Task<RespResult> ExchangeAsync(int retry)
        {
            RespResult ret = RespResult.ErrorTimeout;
            for (int i = 0; i < retry && NeedRetry(ret); ++i)
            {
                DebugLog.WriteLine("START transaction, try " + i.ToString());
                if (State != ConnectionState.Connected)
                    return RespResult.ErrorConnection;
                ret = await SingleExchangeAsync();
                DebugLog.WriteLine("END transaction, try " + i.ToString());
            }
            return ret;
        }
        private async Task<bool> DoReadMemoryAsync(uint addr_offset, uint mem_size
            , byte[] dst, int dst_start
            , Action<float> onStepProgress, CancellationToken cancellationToken)
        {
            if (null == dst || dst.Length < (int)mem_size)
                throw new Exception("dst is too short");
            uint step_count = mem_size / MaxReqLen + 1;
            float sep_cost = 1.0f / step_count;
            float progress = 0.0f;
            _TxBuf[0] = 0x0D;
            _TxBuf[1] = 0x0A;
            _TxBuf[2] = Address;
            _TxBuf[3] = 0x01; //read

            uint curr_addr = 0;
            while (mem_size > curr_addr)
            {
                uint addr = curr_addr + addr_offset;
                ushort curr_len = MaxReqLen;
                if (curr_addr + curr_len > mem_size)
                    curr_len = (ushort)(mem_size - curr_addr);

                BitConverter.GetBytes(addr).CopyTo(_TxBuf.AsSpan(4, 4)); //addr
                BitConverter.GetBytes(curr_len).CopyTo(_TxBuf.AsSpan(8, 2)); //len
                byte[] crc = CrcModbusCalculator.ModbusCrc(_TxBuf, 2, 8);
                crc.CopyTo(_TxBuf.AsSpan(10, 2)); //crc
                _EndTxBuf = 12;
                RespResult ret = await ExchangeAsync(_Retry);
                if (RespResult.NormalPkg != ret)
                    return false;

                _RxBuf.AsSpan(_BeginRxBuf + 12, curr_len)
                    .CopyTo(dst.AsSpan(dst_start, curr_len));
                curr_addr += curr_len;

                progress += sep_cost;
                onStepProgress?.Invoke(progress);
            }
            return true;
        }
        private async Task<bool> DoWriteMemoryAsync(uint addr_offset, uint mem_size
            , byte[] src, int src_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default)
        {
            if (null == src || src.Length < (int)mem_size)
                throw new Exception("dst is too short");
            uint step_count = mem_size / MaxReqLen + 1;
            float sep_cost = 1.0f / step_count;
            float progress = 0.0f;
            _TxBuf[0] = 0x0D;
            _TxBuf[1] = 0x0A;
            _TxBuf[2] = Address;
            _TxBuf[3] = 0x02; //write

            uint curr_addr = 0;
            while (mem_size > curr_addr)
            {
                uint addr = curr_addr + addr_offset;
                ushort curr_len = MaxReqLen;
                if (curr_addr + curr_len > mem_size)
                    curr_len = (ushort)(mem_size - curr_addr);

                BitConverter.GetBytes(addr).CopyTo(_TxBuf.AsSpan(4, 4)); //addr
                BitConverter.GetBytes(curr_len).CopyTo(_TxBuf.AsSpan(8, 2)); //len
                byte[] crc = CrcModbusCalculator.ModbusCrc(_TxBuf, 2, 8);
                crc.CopyTo(_TxBuf.AsSpan(10, 2)); //crc

                src.AsSpan((int)curr_addr+ src_start, curr_len)
                    .CopyTo(_TxBuf.AsSpan(12 + (int)curr_addr, curr_len));

                crc = CrcModbusCalculator.ModbusCrc(_TxBuf, 12, curr_len);
                crc.CopyTo(_TxBuf.AsSpan(12+ curr_len, 2)); //crc

                _EndTxBuf = 12 + curr_len + 2;

                RespResult ret = await ExchangeAsync(_Retry);
                if (RespResult.NormalPkg != ret)
                    return false;

                curr_addr += curr_len;

                progress += sep_cost;
                onStepProgress?.Invoke(progress);
            }
            return true;
        }
        public override async Task<bool> ReadMemoryAsync(uint start_addr, uint mem_size
            , byte[] dst, int dst_start
            , Action<float> onStepProgress, CancellationToken cancellationToken)
        {
            using (await _semaphore.UseWaitAsync())
            {
                try
                {
                    #if DEBUG
                    _PerfCounter.Restart();
                    #endif
                    return await DoReadMemoryAsync(start_addr, mem_size
                        , dst, dst_start, onStepProgress, cancellationToken);
                }
                finally
                {
                    #if DEBUG
                    _PerfCounter.Stop();
                    #endif
                }
            }
        }
        public override async Task<bool> WriteMemoryAsync(uint start_addr, uint mem_size
            , byte[] src, int src_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default)
        {
            using (await _semaphore.UseWaitAsync())
            {
                try
                {
                    #if DEBUG
                    _PerfCounter.Restart();
                    #endif
                    return await DoWriteMemoryAsync(start_addr, mem_size
                        , src, src_start, onStepProgress, cancellationToken);
                }
                finally
                {
                    #if DEBUG
                    _PerfCounter.Stop();
                    #endif
                }
            }
        }

        public override Task<byte[]> Exchange(byte[] req)
        {
            throw new NotImplementedException();
        }
    }
}
