#define DEBUG_UNIT
using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Adapters;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    abstract public class SiamProtocolConnection : IProtocolConnection , INotifyPropertyChanged
    {
        private static Logger mLogger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected IConnection mBaseConn=null;
        protected ConnectionState mState = ConnectionState.Disconnected;
        public SiamProtocolConnection(IConnection base_conn)
        {
            mBaseConn = base_conn;
        }
        public IPhyInterface PhyInterface => mBaseConn.PhyInterface;

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
            get { return mState; }
            private set 
            {
                mState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
                DebugLog.WriteLine("Connect State="+ mState.ToString());
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
                if(result)
                {
                    if(ConnectionState.Connected != State)
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
        public virtual async Task Disconnect()
        {
            State = ConnectionState.PendingDisconnect;
            try
            {
                mBaseConn.Disconnect();
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
        }

        private TaskCompletionSource<bool> mExecTcs;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private byte[] mRxBuf = new byte[512];
        private DataBuffer mBuf = new DataBuffer();
        
        public const int mExchangeRetry = 3;
        public const int mRequestRetry = 3;
        #if DEBUG
        public const int mResponseRetry = 100;
        #else
        public const int mResponseRetry = 256;
        #endif

        private const int mMinSpeed = 1200; ///bit per second
        private const float multipler = 1000.0f / (mMinSpeed / (8 + 1 + 1)) ;
        private static int GetTime(int bytes)
        {
            // 1000000usec
            // mMinSpeed/(8+1+1) - byte per second
            //one byte = 1000000 / (9600 / (8 + 1 + 1)) / 1000 = 1,04166msec
            //int timeout = 1000000 / (mMinSpeed / (8 + 1 + 1)) * bytes / 1000;
            
            int timeout = (int)(bytes* multipler + 200);
            if (1 > timeout)
                timeout = 1;
            return timeout;
        }

        public static int GetRequestTimeout(byte[] rq)
        {
            if (null == rq)
                return 0;
            return GetTime(rq.Length);
        }

        public static int GetResponseTimeout(byte[] rq)
        {
            if (null == rq)
                return 0;

            switch (rq[3])
            {
                default: break;
                case 0x01:
                    UInt16 data_len = BitConverter.ToUInt16(rq, 8);
                    return GetTime(rq.Length + data_len);
                case 0x02:
                    return GetTime(rq.Length);
            }
            return 0;
        }

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
            int write_timeout = GetRequestTimeout(data);
            CancellationTokenSource ctSrc = new CancellationTokenSource(write_timeout);
            bool sent_ok = false;
            for (int i = 0; i < mRequestRetry && !sent_ok; ++i)
            {
                try
                {
                    ctSrc.Token.ThrowIfCancellationRequested();
                    int sent = await mBaseConn.WriteAsync(data, 0, data.Length, ctSrc.Token);
                    if(data.Length == sent)
                        sent_ok = true;
                    DebugLog.WriteLine("Sent " + data.Length.ToString() +
                        ": [" + BitConverter.ToString(data) + "]\n");
                }
                catch (OperationCanceledException ex)
                {
                    DebugLog.WriteLine("Request timeout: rq=" + BitConverter.ToString(data));
                    throw ex;
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Request ERROR - "
                    + BitConverter.ToString(data));
                    throw ex;
                }
            }
            //if (!sent)
            //    ConnectFailed?.Invoke();
            return sent_ok;
        }
        private async Task<byte[]> ResponseAsync(byte[] req)
        {
            int read_timeout = GetResponseTimeout(req);
            Stopwatch perf_counter = new Stopwatch();
            perf_counter.Start();
            
            CancellationTokenSource ctSrc = new CancellationTokenSource(read_timeout);
            byte[] pkg = { };
            for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            {
                try
                {
                    ctSrc.Token.ThrowIfCancellationRequested();
                    pkg = mBuf.Extract();
                    if (0 == pkg.Length)
                    {
                        int readed = await mBaseConn.ReadAsync(mRxBuf, 0, mRxBuf.Length, ctSrc.Token);
                        if(0 < readed)
                            mBuf.Append(mRxBuf, readed);
                        DebugLog.WriteLine("Appended bytes="+ readed.ToString()
                            +" elapsed=" + perf_counter.ElapsedMilliseconds.ToString()
                            + " / " + read_timeout.ToString()
                            + ": [" + BitConverter.ToString(mRxBuf, 0, readed) + "]\n");
                    }
                    else
                    {
                        int cmp = req.AsSpan().Slice(0, 12).SequenceCompareTo(pkg.AsSpan().Slice(0, 12));
                        if (0 != cmp)
                        {
                            DebugLog.WriteLine("WRONG response"
                                + " elapsed=" + perf_counter.ElapsedMilliseconds.ToString()
                                + " / " + read_timeout.ToString()
                                + ": [" + BitConverter.ToString(pkg) + "]\n");
                            pkg = new byte[] { };
                        }
                        else
                        {
                            DebugLog.WriteLine("OK response"
                                + " elapsed=" + perf_counter.ElapsedMilliseconds.ToString()
                                + " / " + read_timeout.ToString()
                                + ": [" + BitConverter.ToString(pkg) + "]\n");
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    DebugLog.WriteLine("Response timeout "
                        + " elapsed=" + perf_counter.ElapsedMilliseconds.ToString()
                        + " / " + read_timeout.ToString()
                        + ": [" + BitConverter.ToString(pkg) + "]\n");
                    throw ex;
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Response ERROR "
                        + " elapsed=" + perf_counter.ElapsedMilliseconds.ToString()
                        + " / " + read_timeout.ToString());
                    throw ex;
                }
            }
            return pkg;
        }
        private async Task<byte[]> SingleExchangeAsync(byte[] req)
        {
            if (State != ConnectionState.Connected)
                throw new Exception("No connection exception");
            byte[] res = { };
            try
            {
                mBuf.Clear();
                mBaseConn.ClearRx();
                mBaseConn.ClearTx();
                bool sent = await RequestAsync(req);
                if (sent)
                {
                    //Debug.WriteLine("start wait response");
                    res = await ResponseAsync(req);
                }
            }
            catch (OperationCanceledException ex)
            {
                DebugLog.WriteLine("Exchange canceled by timeout ");
                // dont rethrow 
            }
            catch (Exception ex)
            {
                /*
                DebugLog.WriteLine("Exception in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                */
                throw ex;
            }
            return res;
        }
        private async Task<byte[]> ExchangeData(byte[] req)
        {
            byte[] res = { };
            if (State != ConnectionState.Connected)
                return res;

            try
            {
                for (int i = 0; i < mExchangeRetry && 0 == res.Length; ++i) 
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
                
                DebugLog.WriteLine("ExchangeData Do disconnect ");
                //await Disconnect();
                //await Connect();
            }
            return res;
        }
        public async Task<byte[]> Exchange(byte[] req)
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
                    ret = await ExchangeData(req);
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
