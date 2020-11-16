#define DEBUG_UNIT
using SiamCross.Models.Adapters;
using SiamCross.Models.Tools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    abstract public class SiamProtocolConnection : IProtocolConnection
    {
        protected IConnection mBaseConn;
        public SiamProtocolConnection(IConnection base_conn)
        {
            mBaseConn = base_conn;
        }
        public IPhyInterface PhyInterface => mBaseConn.PhyInterface;
        public virtual Task<bool> Connect()
        {
            return mBaseConn.Connect();
        }
        public virtual Task Disconnect()
        {
            return mBaseConn.Disconnect();
        }

        private TaskCompletionSource<bool> mExecTcs;
        private byte[] mRxBuf = new byte[512];
        private DataBuffer mBuf = new DataBuffer();
        
        public const int mExchangeRetry = 3;
        public const int mRequestRetry = 3;
        #if DEBUG
        public const int mExchangeTimeout = 10000;
        public const int mResponseRetry = 10;
        #else
        public const int mExchangeTimeout = 3000;
        public const int mResponseRetry = 256;
        #endif


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
        private async Task<bool> RequestAsync(byte[] data, CancellationToken ct)
        {
            bool sent_ok = false;
            for (int i = 0; i < mRequestRetry && !sent_ok; ++i)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    int sent = await mBaseConn.WriteAsync(data, 0, data.Length, ct);
                    if(data.Length == sent)
                        sent_ok = true;
                    DebugLog.WriteLine("Sent " + data.Length.ToString() +
                        ": [" + BitConverter.ToString(data) + "]\n");
                }
                catch (Exception sendingEx)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Ошибка отправки BLE : "
                   + BitConverter.ToString(data)
                   + " " + sendingEx.Message + " "
                   + sendingEx.GetType() + " "
                   + sendingEx.StackTrace + "\n");
                }
            }
            //if (!sent)
            //    ConnectFailed?.Invoke();
            return sent_ok;
        }
        private async Task<byte[]> ResponseAsync(byte[] req, CancellationToken ct)
        {
            byte[] pkg = { };
            for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    pkg = mBuf.Extract();
                    if (0 == pkg.Length)
                    {
                        int readed = await mBaseConn.ReadAsync(mRxBuf, 0, mRxBuf.Length, ct);
                        if(0 < readed)
                            mBuf.Append(mRxBuf, readed);
                    }
                    else
                    {
                        int cmp = req.AsSpan().Slice(0, 12).SequenceCompareTo(pkg.AsSpan().Slice(0, 12));
                        if (0 != cmp)
                        {
                            DebugLog.WriteLine("WRONG response" +
                                ": [" + BitConverter.ToString(pkg) + "]\n");
                            pkg = new byte[] { };
                        }
                        else
                        {
                            DebugLog.WriteLine("OK response" +
                                ": [" + BitConverter.ToString(pkg) + "]\n");
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    DebugLog.WriteLine("Response timeout {0}: {1}", ex.GetType().Name, ex.Message);
                    throw ex;
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Ошибка получения BLE : "
                    + BitConverter.ToString(pkg)
                    + " " + ex.Message + " "
                    + ex.GetType() + " "
                    + ex.StackTrace + "\n");
                }
            }
            return pkg;
        }
        private async Task<byte[]> SingleExchangeAsync(byte[] req)
        {

            byte[] res = { };
            CancellationTokenSource ctSrc = new CancellationTokenSource(mExchangeTimeout);
            try
            {
                mBuf.Clear();
                mBaseConn.ClearRx();
                mBaseConn.ClearTx();
                bool sent = await RequestAsync(req, ctSrc.Token);
                if (sent)
                {
                    //Debug.WriteLine("start wait response");
                    res = await ResponseAsync(req, ctSrc.Token);
                }
            }
            catch (OperationCanceledException ex)
            {
                DebugLog.WriteLine("Exchange canceled by timeout {0}: {1}", ex.GetType().Name, ex.Message);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("UNKNOWN exception in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + " : " + e.Message);
            }
            finally
            {
                ctSrc.Dispose();
            }
            return res;
        }
        private async Task<byte[]> ExchangeData(byte[] req)
        {
            byte[] res = { };
            for (int i = 0; i < mExchangeRetry && 0 == res.Length; ++i)
            {
                DebugLog.WriteLine("START transaction, try " + i.ToString());
                res = await SingleExchangeAsync(req);
                DebugLog.WriteLine("END transaction, try " + i.ToString());
            }
            return res;
        }
        public async Task<byte[]> Exchange(byte[] req)
        {
            Task<byte[]> task = null;
            byte[] ret = { };
            try
            {
                if (null != mExecTcs)
                {
                    DebugLog.WriteLine("WARNING another task running");
                    bool result = await mExecTcs.Task;
                }
                mExecTcs = new TaskCompletionSource<bool>();
                task = ExchangeData(req);
                ret = await task;
            }
            catch (Exception sendingEx)
            {
                DebugLog.WriteLine("WARNING Exchange"
                + " " + sendingEx.Message + " "
                + sendingEx.GetType() + " "
                + sendingEx.StackTrace + "\n");
            }
            finally
            {
                mExecTcs?.TrySetResult(true);
                task?.Dispose();
                task = null;
                mExecTcs = null;
            }
            return ret;
        }
        public async Task SendData(byte[] req)
        {
            byte[] ret = await Exchange(req);

            if (null != ret && 0 < ret.Length)
            {
                DoActionDataReceived(ret);
                //DataReceived?.Invoke(ret);
            }

        }

        public abstract event Action<byte[]> DataReceived;
        public abstract event Action ConnectSucceed;
        public abstract event Action ConnectFailed;

        public abstract void DoActionDataReceived(byte[] data);
        public abstract void DoActionConnectSucceed();
        public abstract void DoActionConnectFailed();
    }
}
