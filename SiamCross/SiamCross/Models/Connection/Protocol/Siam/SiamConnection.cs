﻿#define DEBUG_UNIT
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol.Siam
{
    [Fody.ConfigureAwait(false)]
    public class SiamConnection : BaseProtocol
    {


        public SiamConnection(IPhyConnection base_conn, byte address = 1)
            : base(base_conn, address)
        {
            _TxBuf[0] = 0x0D;
            _TxBuf[1] = 0x0A;
            _TxBuf[2] = base.Address;
        }

        public override async Task<bool> Connect(CancellationToken ct)
        {
            using (await _semaphore.UseWaitAsync(ct))
            {
                RequestCount = 0;
                ResponseCount = 0;
                return await base.Connect(ct);
            }
        }
        public override async Task<bool> Disconnect()
        {
            using (await _semaphore.UseWaitAsync())
                return await base.Disconnect();
        }
        public override byte Address
        {
            get => base.Address;
            set
            {
                base.Address = value;
                _TxBuf[2] = base.Address;
            }
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly byte[] _RxBuf = new byte[Constants.MAX_PKG_SIZE * 2];
        private int _BeginRxBuf = 0;
        private int _EndRxBuf = 0;
        private readonly byte[] _TxBuf = new byte[Constants.MAX_PKG_SIZE];
        private int _EndTxBuf = 0;

        private static readonly int _ResponseRetry = Constants.MAX_PKG_SIZE;


        private static readonly int mMinSpeed = 56700; ///bit per second
        private static readonly float multipler = 1000.0f / (mMinSpeed / (8 + 1 + 1));
        private static int GetTime(int bytes)
        {
            // 1000000usec
            // mMinSpeed/(8+1+1) - byte per second
            //one byte = 1000000 / (9600 / (8 + 1 + 1)) / 1000 = 1,04166msec
            //int timeout = 1000000 / (mMinSpeed / (8 + 1 + 1)) * bytes / 1000;
            int timeout = (int)(bytes * multipler + 1);
            return 20 > timeout ? 20 : timeout;
        }
        private static int GetRequestTimeout(int len)
        {
            return GetTime(len);
        }
        private int GetResponseTimeout(byte[] rq, int len)
        {
            int timeout = 0;
            if (null == rq)
                return timeout;
            switch (rq[3])
            {
                default: break;
                case 0x01:
                    ushort data_len = BitConverter.ToUInt16(rq, 8);
                    timeout = GetTime(len + data_len + 2);
                    break;
                case 0x02:
                    timeout = GetTime(len);
                    break;
            }
            return timeout + mAdditioonTime;
        }

        private readonly Stopwatch _PerfCounter = new Stopwatch();

        private void CheckEmptySpace()
        {
            int empty_space = _RxBuf.Length - _EndRxBuf;
            if (0 >= empty_space)
                throw new System.IO.InternalBufferOverflowException();

            if (Constants.MAX_PKG_SIZE > empty_space)
            {
                DebugLog.WriteLine($"MemMove in buffer begin={_BeginRxBuf} end={_EndRxBuf}");
                Span<byte> dst_buf = _RxBuf;
                Span<byte> src_buf = _RxBuf.AsSpan().Slice(_BeginRxBuf, _EndRxBuf);
                src_buf.CopyTo(dst_buf);
            }
        }
        private async Task<bool> RequestAsync(CancellationToken ct)
        {
            bool sent_ok = false;
            try
            {
                for (int i = 0; i < Retry && !sent_ok; ++i)
                {
                    int sent = await mPhyConn.WriteAsync(_TxBuf, 0, _EndTxBuf, ct);
                    if (_EndTxBuf == sent)
                        sent_ok = true;
                    DebugLog.WriteLine("SENT " + _EndTxBuf.ToString()
                        + " elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString()
                        + ": [" + BitConverter.ToString(_TxBuf, 0, _EndTxBuf) + "]\n");
                }
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("EXCEPTION in write"
                    + " elapsed=" + _PerfCounter.ElapsedMilliseconds.ToString()
                    + ": [" + BitConverter.ToString(_TxBuf, 0, _EndTxBuf) + "]\n"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            RequestCount++;
            return sent_ok;
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
        private async Task<RespResult> ResponseAsync(CancellationToken ct)
        {
            int read_timeout = GetResponseTimeout(_TxBuf, _EndTxBuf);
            _BeginRxBuf = 0;
            _EndRxBuf = 0;
            int need = Constants.MIN_PKG_SIZE;
            bool is_ok = false;
            try
            {
                for (int i = 0; i < _ResponseRetry && !is_ok; ++i)
                {
                    CheckEmptySpace();
                    int readed = await mPhyConn.ReadAsync(_RxBuf, _EndRxBuf, _RxBuf.Length - _EndRxBuf, ct);
                    _EndRxBuf += readed;
                    DebugLog.WriteLine($"Appended bytes={readed} elapsed={_PerfCounter.ElapsedMilliseconds}/{read_timeout}"
                        + $" begin={_BeginRxBuf} end={_EndRxBuf}");
                    Pkg.Extract(_TxBuf, _RxBuf, ref _BeginRxBuf, _EndRxBuf, ref need);
                    if (-2 > need)
                        return RespResult.ErrorUnknown;
                    switch (need)
                    {
                        default:
                            break;
                        case -2:
                        case -1: _BeginRxBuf = 0; goto case 0;
                        case 0:
                            DebugLog.WriteLine($"GET response {_EndRxBuf - _BeginRxBuf} " + ((RespResult)need).ToString()
                                + $" elapsed={_PerfCounter.ElapsedMilliseconds}/{read_timeout}"
                                + ": [" + BitConverter.ToString(_RxBuf, _BeginRxBuf, _EndRxBuf - _BeginRxBuf) + "]\n"
                                //+ ": [" + BitConverter.ToString(_RxBuf, 0, _EndRxBuf) + "]\n"
                                );
                            ResponseCount++;
                            return ((RespResult)need);
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
            return RespResult.ErrorTimeout;

        }
        private async Task<RespResult> SingleExchangeAsync(CancellationToken ct)
        {
            try
            {
                if (PhyConnection.State != ConnectionState.Connected)
                    if (!await base.Connect(ct))
                        return RespResult.ErrorConnection;

                await mPhyConn.ClearRx();
                await mPhyConn.ClearTx();
                bool sent = false;
                using (var ctSrc = new CancellationTokenSource(GetRequestTimeout(_EndTxBuf)))
                {
                    using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                    {
                        sent = await RequestAsync(linkTsc.Token);
                    }
                }
                if (!sent)
                    return RespResult.ErrorSending;
                using (var ctSrc = new CancellationTokenSource(GetResponseTimeout(_TxBuf, _EndTxBuf)))
                {
                    using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                    {
                        return await ResponseAsync(linkTsc.Token);
                    }
                }

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
                case RespResult.ErrorUnknown:
                case RespResult.ErrorPkg:
                case RespResult.NormalPkg: return false;

                case RespResult.ErrorConnection:
                case RespResult.ErrorSending:
                case RespResult.ErrorTimeout:
                case RespResult.ErrorCrc: return true;
            }
        }
        private async Task<RespResult> ExchangeAsync(int retry, CancellationToken ct)
        {
            RespResult ret = RespResult.ErrorTimeout;
            for (int i = 0; i < retry && NeedRetry(ret) && !ct.IsCancellationRequested; ++i)
            {
                try
                {
#if DEBUG
                    _PerfCounter.Restart();
#endif
                    DebugLog.WriteLine("START transaction, try " + i.ToString());
                    ret = await SingleExchangeAsync(ct);
                    DebugLog.WriteLine("END transaction, try " + i.ToString());
                }
                finally
                {
#if DEBUG
                    _PerfCounter.Stop();
#endif
                }
            }
            return ret;
        }
        private async Task<RespResult> DoReadMemoryAsync(uint addr_offset, uint mem_size
            , byte[] dst, int dst_start
            , Action<uint> onStepProgress, CancellationToken ct)
        {
            if (null == dst || dst.Length < (int)mem_size)
                return RespResult.ErrorUnknown;
            //_TxBuf[0] = 0x0D;
            //_TxBuf[1] = 0x0A;
            //_TxBuf[2] = Address;
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
                RespResult ret = await ExchangeAsync(Retry, ct);
                if (RespResult.NormalPkg != ret)
                    return ret;

                _RxBuf.AsSpan(_BeginRxBuf + 12, curr_len)
                    .CopyTo(dst.AsSpan(dst_start + (int)curr_addr, curr_len));
                curr_addr += curr_len;

                onStepProgress?.Invoke(curr_len);
            }
            return RespResult.NormalPkg;
        }
        private async Task<RespResult> DoWriteMemoryAsync(uint addr_offset, uint mem_size
            , byte[] src, int src_start
            , Action<uint> onStepProgress, CancellationToken ct)
        {
            if (null == src || src.Length < (int)mem_size)
                return RespResult.ErrorUnknown;
            //throw new Exception("dst is too short");
            //_TxBuf[0] = 0x0D;
            //_TxBuf[1] = 0x0A;
            //_TxBuf[2] = Address;
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

                src.AsSpan((int)curr_addr + src_start, curr_len)
                    .CopyTo(_TxBuf.AsSpan(12 + (int)curr_addr, curr_len));

                crc = CrcModbusCalculator.ModbusCrc(_TxBuf, 12, curr_len);
                crc.CopyTo(_TxBuf.AsSpan(12 + curr_len, 2)); //crc

                _EndTxBuf = 12 + curr_len + 2;

                RespResult ret = await ExchangeAsync(Retry, ct);
                if (RespResult.NormalPkg != ret)
                    return ret;

                curr_addr += curr_len;

                onStepProgress?.Invoke(curr_len);
            }
            return RespResult.NormalPkg;
        }
        public override async Task<RespResult> TryReadMemoryAsync(uint start_addr, uint mem_size
            , byte[] dst, int dst_start
            , Action<uint> onStepProgress, CancellationToken cancellationToken)
        {
            using (await _semaphore.UseWaitAsync(cancellationToken))
            {
                return await DoReadMemoryAsync(start_addr, mem_size
                    , dst, dst_start, onStepProgress, cancellationToken);
            }
        }
        public override async Task<RespResult> TryWriteMemoryAsync(uint start_addr, uint mem_size
            , byte[] src, int src_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default)
        {
            using (await _semaphore.UseWaitAsync(cancellationToken))
            {
                return await DoWriteMemoryAsync(start_addr, mem_size
                    , src, src_start, onStepProgress, cancellationToken);
            }
        }

        private async Task<RespResult> DoReadVarAsync(MemStruct mem
            , Action<uint> onStepProgress, CancellationToken ct)
        {
            float sep_byte_cost = 1.0f / mem.Size;
            MemStruct vars = new MemStruct(0);

            var vdict = mem.GetVars();
            var enumerator = vdict.GetEnumerator();

            bool has_next = enumerator.MoveNext();

            while (has_next)
            {
                var curr = enumerator.Current;
                vars.Reset(curr.Address);
                while (has_next && vars.Size + curr.Size <= MaxReqLen)
                {
                    vars.Add(new MemVar(curr.Data));
                    has_next = enumerator.MoveNext();
                    curr = enumerator.Current;
                }
                //MakeReadRequest(vars.Address, vars.Size);
                _TxBuf[3] = 0x01; //read
                BitConverter.GetBytes(vars.Address).CopyTo(_TxBuf.AsSpan(4, 4)); //addr
                BitConverter.GetBytes((UInt16)vars.Size).CopyTo(_TxBuf.AsSpan(8, 2)); //len
                CrcModbusCalculator.ModbusCrc(_TxBuf, 2, 8).CopyTo(_TxBuf.AsSpan(10, 2)); //crc
                _EndTxBuf = 12;

                RespResult ret = await ExchangeAsync(Retry, ct);
                if (RespResult.NormalPkg != ret)
                    return ret;
                vars.FromArray(_RxBuf, (UInt32)_BeginRxBuf + 12);

                onStepProgress?.Invoke(vars.Size);

            }
            return RespResult.NormalPkg;
        }
        private async Task<RespResult> DoWriteVarAsync(MemStruct mem
            , Action<uint> onStepProgress, CancellationToken ct)
        {
            float sep_byte_cost = 1.0f / mem.Size;
            MemStruct vars = new MemStruct(0);

            var vdict = mem.GetVars();
            var enumerator = vdict.GetEnumerator();

            bool has_next = enumerator.MoveNext();

            while (has_next)
            {
                var curr = enumerator.Current;
                vars.Reset(curr.Address);
                while (has_next && vars.Size + curr.Size <= MaxReqLen)
                {
                    vars.Add(new MemVar(curr.Data));
                    has_next = enumerator.MoveNext();
                    curr = enumerator.Current;
                }
                //MakeReadRequest(vars.Address, vars.Size);
                _TxBuf[3] = 0x02; //write
                BitConverter.GetBytes(vars.Address).CopyTo(_TxBuf.AsSpan(4, 4)); //addr
                BitConverter.GetBytes((UInt16)vars.Size).CopyTo(_TxBuf.AsSpan(8, 2)); //len
                CrcModbusCalculator.ModbusCrc(_TxBuf, 2, 8).CopyTo(_TxBuf.AsSpan(10, 2)); //crc

                vars.ToArray(_TxBuf, 12);

                CrcModbusCalculator.ModbusCrc(_TxBuf, 12, (int)vars.Size).CopyTo(_TxBuf.AsSpan(12 + (int)vars.Size, 2)); //crc
                _EndTxBuf = 12 + (int)vars.Size + 2;

                RespResult ret = await ExchangeAsync(Retry, ct);
                if (RespResult.NormalPkg != ret)
                    return ret;

                onStepProgress?.Invoke(vars.Size);

            }
            return RespResult.NormalPkg;
        }

        public override async Task<RespResult> TryReadAsync(MemStruct var
            , Action<uint> onStepProgress, CancellationToken ct)
        {
            using (await _semaphore.UseWaitAsync(ct))
            {
                return await DoReadVarAsync(var, onStepProgress, ct);
            }
        }
        public override async Task<RespResult> TryWriteAsync(MemStruct var
                , Action<uint> onStepProgress, CancellationToken ct)
        {
            using (await _semaphore.UseWaitAsync(ct))
            {
                return await DoWriteVarAsync(var, onStepProgress, ct);
            }
        }



        public override Task<byte[]> Exchange(byte[] req)
        {
            throw new NotImplementedException();
        }
    }
}
