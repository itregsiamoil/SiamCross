#define DEBUG_UNIT
using SiamCross.Models.Tools;
using SiamCross.Protocol.Siam;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class SiamProtocolConnection : IProtocolConnection, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected IPhyConnection mBaseConn = null;
        protected ConnectionState mState = ConnectionState.Disconnected;
        public SiamProtocolConnection(IPhyConnection base_conn)
        {
            mBaseConn = base_conn;
            MaxReqLen = 200;
        }
        public IPhyConnection PhyConnection => mBaseConn;

        public void UpdateRssi()
        {
            if (null == mBaseConn)
                return;
            mBaseConn.UpdateRssi();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Rssi"));
        }
        public int Rssi
        {
            get
            {
                if (null == mBaseConn)
                    return 0;
                return mBaseConn.Rssi;
            }
        }
        public ConnectionState State
        {
            get => mState;
            private set
            {
                mState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
                DebugLog.WriteLine("Connect State=" + mState.ToString());
            }
        }

        public virtual async Task<bool> Connect()
        {
            State = ConnectionState.PendingConnect;

            bool result = false;
            try
            {
                result = await mBaseConn.Connect();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("Exception in: "
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
                        State = ConnectionState.Connected;
                }
                else
                {
                    if (ConnectionState.Disconnected != State)
                        State = ConnectionState.Disconnected;
                }

            }
            return result;
        }
        public virtual async Task<bool> Disconnect()
        {
            bool ret = false;
            State = ConnectionState.PendingDisconnect;
            try
            {
                ret = await mBaseConn.Disconnect();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                State = ConnectionState.Disconnected;
            }
            return ret;
        }

        private TaskCompletionSource<bool> mExecTcs;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private int mBeginRxBuf = 0;
        private int mEndRxBuf = 0;
        private readonly byte[] mRxBuf = new byte[Pkg.MAX_PKG_SIZE * 2];
        private readonly byte[] mTxBuf = new byte[Pkg.MAX_PKG_SIZE];

        private readonly DataBuffer mBuf = new DataBuffer();

        public const int mRequestRetry = 3;
#if DEBUG
        public const int mResponseRetry = 100;
#else
        public const int mResponseRetry = 256;
#endif

        private const int mAdditioonTime = 500;
        private const int mMinSpeed = 9600; ///bit per second
        private const float multipler = 1000.0f / (mMinSpeed / (8 + 1 + 1));
        private static int GetTime(int bytes)
        {
            // 1000000usec
            // mMinSpeed/(8+1+1) - byte per second
            //one byte = 1000000 / (9600 / (8 + 1 + 1)) / 1000 = 1,04166msec
            //int timeout = 1000000 / (mMinSpeed / (8 + 1 + 1)) * bytes / 1000;
            int timeout = (int)(bytes * multipler + 0.5);
            return (1 > timeout) ? 1 : timeout;
        }
        public static int GetRequestTimeout(int len)
        {
            return GetTime(len) + mAdditioonTime;
        }
        public static int GetResponseTimeout(byte[] rq)
        {
            int timeout = 0;
            if (null == rq)
                return timeout;
            switch (rq[3])
            {
                default: break;
                case 0x01:
                    UInt16 data_len = BitConverter.ToUInt16(rq, 8);
                    timeout = GetTime(rq.Length + data_len + 2);
                    break;
                case 0x02:
                    timeout = GetTime(rq.Length);
                    break;
            }
            return timeout;
        }

        private readonly Stopwatch mPerfCounter = new Stopwatch();

        private static void LockLog(string msg)
        {
            string ret;
            Thread thread = Thread.CurrentThread;
            {
                ret = msg
                    + String.Format("   Thread ID: {0} ", thread.ManagedThreadId
                    //+ String.Format("   Background: {0} ", thread.IsBackground)
                    //+ String.Format("   Thread Pool: {0} ", thread.IsThreadPoolThread)
                    );
            }
            DebugLog.WriteLine(ret);
        }

        private async Task<bool> RequestAsync(byte[] data)
        {
            return await RequestAsync(data, data.Length);
        }

        private async Task<bool> RequestAsync(byte[] data, int len)
        {
            int write_timeout = GetRequestTimeout(len);
            CancellationTokenSource ctSrc = new CancellationTokenSource(write_timeout);
            bool sent_ok = false;
            for (int i = 0; i < mRequestRetry && !sent_ok; ++i)
            {
                ctSrc.Token.ThrowIfCancellationRequested();
                int sent = await mBaseConn.WriteAsync(data, 0, len, ctSrc.Token);
                if (len == sent)
                    sent_ok = true;
                DebugLog.WriteLine("Sent " + len.ToString()
                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                    + ": [" + BitConverter.ToString(data, 0, len) + "]\n");
            }
            //if (!sent)
            //    ConnectFailed?.Invoke();
            return sent_ok;
        }
        private async Task<byte[]> ResponseAsync(byte[] req)
        {
            int pf_delay = GetResponseTimeout(req);
            int read_timeout = GetResponseTimeout(req) + mAdditioonTime;
            CancellationTokenSource ctSrc = new CancellationTokenSource(read_timeout);
            // делаем минимальную задержку чтоб принять как минимум заоловок пакета
            // без ожидания
            //DebugLog.WriteLine($"Prefetch delay = {pf_delay}");
            //await Task.Delay(pf_delay, ctSrc.Token);

            int single_read;
            byte[] pkg = { };
            try
            {
                for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
                {
                    //DebugLog.WriteLine("Begin ReadAsync");
                    single_read = await mBaseConn.ReadAsync(mRxBuf, 0, mRxBuf.Length, ctSrc.Token);
                    //DebugLog.WriteLine($"End ReadAsync readed={readed}");
                    if (0 < single_read)
                    {
                        mBuf.Append(mRxBuf, single_read);
                        DebugLog.WriteLine("Appended bytes=" + single_read.ToString()
                            + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                            + " / " + read_timeout.ToString());
                        //+ ": [" + BitConverter.ToString(mRxBuf, 0, single_read) + "]\n");
                        pkg = mBuf.Extract();
                        if (0 != pkg.Length)
                        {
                            int cmp = req.AsSpan().Slice(0, 10).SequenceCompareTo(pkg.AsSpan().Slice(0, 10));
                            if (0 != cmp)
                            {
                                DebugLog.WriteLine("WRONG response"
                                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                                    + " / " + read_timeout.ToString()
                                    + ": [" + BitConverter.ToString(pkg) + "]\n");
                                pkg = new byte[] { };
                            }
                            else
                            {
                                DebugLog.WriteLine("OK response"
                                    + " expected=" + pf_delay.ToString()
                                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                                    + " / " + read_timeout.ToString()
                                    + ": [" + BitConverter.ToString(pkg) + "]\n");
                            }
                        } // if (0 != pkg.Length)
                    } // if (0 < single_read)
                }//for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("EXCEPTION response"
                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                    + " / " + read_timeout.ToString()
                    + ": [" + BitConverter.ToString(pkg) + "]\n");
                throw ex;
            }
            finally
            {
                ctSrc.Dispose();
            }
            return pkg;
        }
        private async Task<byte[]> SingleExchangeAsync(byte[] req)
        {
            try
            {
                mBuf.Clear();
                mBaseConn.ClearRx();
                mBaseConn.ClearTx();
                bool sent = await RequestAsync(req);
                if (sent)
                {
                    //Debug.WriteLine("start wait response");
                    return await ResponseAsync(req);
                }
            }
            catch (OperationCanceledException)
            {
                DebugLog.WriteLine("Exchange canceled by timeout disconnect");
            }
            return new byte[] { };
        }
        private async Task<byte[]> ExchangeData(byte[] req, int retry)
        {
            byte[] res = { };
            if (State != ConnectionState.Connected)
                return res;
            try
            {
                for (int i = 0; i < retry && 0 == res.Length; ++i)
                {
                    DebugLog.WriteLine("START transaction, try " + i.ToString());
                    res = await SingleExchangeAsync(req);
                    DebugLog.WriteLine("END transaction, try " + i.ToString());
                }
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("ExchangeData ERROR "
                + System.Reflection.MethodBase.GetCurrentMethod().Name
                + "\n msg=" + ex.Message
                + "\n type=" + ex.GetType()
                + "\n stack=" + ex.StackTrace + "\n");
            }
            return res;
        }

        public async Task<byte[]> Exchange(byte[] req)
        {
#if DEBUG
            mPerfCounter.Restart();
            byte[] res = await Exchange(req, 3);
            mPerfCounter.Stop();
            return res;
#else
            return await Exchange(req, 3);
#endif
        }
        public async Task<byte[]> Exchange(byte[] req, int retry)
        {
            byte[] ret = { };
            try
            {
                using (await semaphore.UseWaitAsync()) //lock (lockObj)
                {
                    if (null != mExecTcs)
                    {
                        DebugLog.WriteLine("WARNING another task running");
                        bool result = await mExecTcs.Task;
                    }
                    mExecTcs = new TaskCompletionSource<bool>();
                    ret = await ExchangeData(req, retry);
                }

            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("Exchange ERROR"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                mExecTcs?.TrySetResult(true);
                mExecTcs = null;
            }
            DebugLog.WriteLine("Total"
                + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString());
            return ret;
        }




        private void CheckEmptySpace(ref int begin, ref int end)
        {
            int empty_space = mRxBuf.Length - end;
            if (empty_space < Pkg.MAX_PKG_SIZE)
            {
                Span<byte> dst_buf = mRxBuf;
                Span<byte> src_buf = mRxBuf.AsSpan().Slice(begin, end);
                src_buf.CopyTo(dst_buf);
            }
        }

        private enum RespResult
        {
            NormalPkg = 0
          , ErrorCrc = -1
          , ErrorPkg = -2
          , ErrorTimeout = -3
          , ErrorSending = -4
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
        private async Task<RespResult> ResponseAsync2()
        {
            int pf_delay = GetResponseTimeout(mTxBuf);
            int read_timeout = GetResponseTimeout(mTxBuf) + mAdditioonTime;
            CancellationTokenSource ctSrc = new CancellationTokenSource(read_timeout);

            mBeginRxBuf = 0;
            mEndRxBuf = 0;
            int need = Pkg.MIN_PKG_SIZE;
            bool is_ok = false;
            try
            {
                for (int i = 0; i < mResponseRetry && !is_ok; ++i)
                {
                    CheckEmptySpace(ref mBeginRxBuf, ref mEndRxBuf);
                    int readed = await mBaseConn.ReadAsync(mRxBuf, mEndRxBuf, mRxBuf.Length - mEndRxBuf, ctSrc.Token);
                    mEndRxBuf += readed;
                    DebugLog.WriteLine($"Appended bytes={readed} elapsed={mPerfCounter.ElapsedMilliseconds}/{read_timeout}");
                    Pkg.Extract(mTxBuf, mRxBuf, ref mBeginRxBuf, mEndRxBuf, ref need);
                    if (-2 > need)
                        return RespResult.ErrorUnknown;
                    switch (need)
                    {
                        default:
                            break;
                        case 0:
                            DebugLog.WriteLine($"GET response "
                                + $" expected={pf_delay}"
                                + $" elapsed={mPerfCounter.ElapsedMilliseconds}/{read_timeout}"
                                + ": [" + BitConverter.ToString(mRxBuf, 0, mEndRxBuf) + "]\n");
                            return RespResult.NormalPkg;
                        case -1: return RespResult.ErrorCrc;
                        case -2: return RespResult.ErrorPkg;
                    }
                }//for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            }
            catch (OperationCanceledException)
            {
                DebugLog.WriteLine("Timeout response"
                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                    + " / " + read_timeout.ToString()
                    + ": [" + BitConverter.ToString(mRxBuf, 0, mEndRxBuf) + "]\n");
            }
            finally
            {
                ctSrc.Dispose();
            }
            return RespResult.ErrorTimeout;

        }
        private async Task<RespResult> SingleExchangeAsync2(int len)
        {
            try
            {
                mBuf.Clear();
                mBaseConn.ClearRx();
                mBaseConn.ClearTx();
                bool sent = await RequestAsync(mTxBuf, len);
                if (!sent)
                    return RespResult.ErrorSending;
                return await ResponseAsync2();
            }
            catch (Exception)
            {
                DebugLog.WriteLine("Unknown EXCEPTION in response"
                    + " elapsed=" + mPerfCounter.ElapsedMilliseconds.ToString()
                    + ": [" + BitConverter.ToString(mRxBuf, 0, mEndRxBuf) + "]\n");
            }
            return RespResult.ErrorUnknown;
        }

        public UInt16 MaxReqLen { get; set; }
        public async Task<bool> ReadMemoryAsync(byte device_addr
            , UInt32 start_addr, UInt32 mem_size
            , byte[] dst, int dst_start
            , Action<float> onStepProgress, CancellationToken cancellationToken)
        {
            using (await semaphore.UseWaitAsync())
            {
                try
                {
#if DEBUG
                    mPerfCounter.Restart();
#endif
                    return await DoReadMemoryAsync(device_addr, start_addr, mem_size
                        , dst, dst_start, onStepProgress, cancellationToken);
                }
                finally
                {
#if DEBUG
                    mPerfCounter.Stop();
#endif
                }
            }
        }

        private async Task<bool> DoReadMemoryAsync(byte device_addr
            , UInt32 addr_offset, UInt32 mem_size
            , byte[] dst, int dst_start
            , Action<float> onStepProgress, CancellationToken cancellationToken)
        {
            const int retry = 3;
            if (null == dst || dst.Length < (int)mem_size)
                throw new Exception("dst is too short");
            UInt32 step_count = mem_size / MaxReqLen + 1;
            float sep_cost = 1.0f / step_count;
            mTxBuf[0] = 0x0D;
            mTxBuf[1] = 0x0A;
            mTxBuf[2] = device_addr;
            mTxBuf[3] = 0x01; //read

            UInt32 curr_addr = 0;
            while (mem_size > curr_addr)
            {
                UInt32 addr = curr_addr + addr_offset;
                UInt16 curr_len = MaxReqLen;
                if (curr_addr + curr_len > mem_size)
                    curr_len = (UInt16)(mem_size - curr_addr);

                BitConverter.GetBytes(addr).CopyTo(mTxBuf.AsSpan(4, 4)); //addr
                BitConverter.GetBytes(curr_len).CopyTo(mTxBuf.AsSpan(8, 2)); //len
                byte[] crc = CrcModbusCalculator.ModbusCrc(mTxBuf, 2, 8);
                crc.CopyTo(mTxBuf.AsSpan(10, 2)); //crc

                bool is_ok = false;
                for (int i = 0; i < retry && !is_ok; ++i)
                {
                    RespResult ret = await SingleExchangeAsync2(12);
                    switch (ret)
                    {
                        case RespResult.NormalPkg:
                            int data_len = Pkg.GetDataLen(mTxBuf);
                            mRxBuf.AsSpan(mBeginRxBuf + 12, data_len)
                                .CopyTo(dst.AsSpan(dst_start, data_len));
                            onStepProgress?.Invoke(sep_cost);
                            is_ok = true;
                            break;
                        default:
                            is_ok = false;
                            break;
                    }
                }
                if (!is_ok)
                    return false;
                curr_addr += curr_len;
            }
            return true;
        }

        public async Task<UInt32> WriteMemoryAsync(UInt32 addr, byte[] data, UInt16 step_len
            , CancellationToken cancellationToken, Action<float> DoStepProgress)
        {
            return 0;
        }
    }
}
