﻿#define DEBUG_UNIT
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol.Siam
{
    public class SiamProtocolConnection : BaseProtocol
    {
        public SiamProtocolConnection(IPhyConnection base_conn, byte address = 1)
            : base(base_conn, address)
        {
        }

        private TaskCompletionSource<bool> mExecTcs;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private readonly byte[] mRxBuf = new byte[Constants.MAX_PKG_SIZE * 2];

        private readonly DataBuffer mBuf = new DataBuffer();

        public const int mRequestRetry = 3;
#if DEBUG
        public const int mResponseRetry = 100;
#else
        public const int mResponseRetry = 256;
#endif

        private const int mMinSpeed = 9600; ///bit per second
        private const float multipler = 1000.0f / (mMinSpeed / (8 + 1 + 1));
        private static int GetTime(int bytes)
        {
            // 1000000usec
            // mMinSpeed/(8+1+1) - byte per second
            //one byte = 1000000 / (9600 / (8 + 1 + 1)) / 1000 = 1,04166msec
            //int timeout = 1000000 / (mMinSpeed / (8 + 1 + 1)) * bytes / 1000;
            int timeout = (int)(bytes * multipler + 0.5);
            return 1 > timeout ? 1 : timeout;
        }
        public static int GetRequestTimeout(int len)
        {
            return GetTime(len);
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
                    ushort data_len = BitConverter.ToUInt16(rq, 8);
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
                    + string.Format("   Thread ID: {0} ", thread.ManagedThreadId
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
            int write_timeout = GetRequestTimeout(len) + mAdditioonTime;
            CancellationTokenSource ctSrc = new CancellationTokenSource(write_timeout);
            bool sent_ok = false;
            for (int i = 0; i < mRequestRetry && !sent_ok; ++i)
            {
                ctSrc.Token.ThrowIfCancellationRequested();
                int sent = await mPhyConn.WriteAsync(data, 0, len, ctSrc.Token);
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
                    single_read = await mPhyConn.ReadAsync(mRxBuf, 0, mRxBuf.Length, ctSrc.Token);
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
                await mPhyConn.ClearRx();
                await mPhyConn.ClearTx();
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

        public override async Task<byte[]> Exchange(byte[] req)
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

        public override Task<RespResult> TryReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<RespResult> TryWriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<uint> onStepProgress = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
