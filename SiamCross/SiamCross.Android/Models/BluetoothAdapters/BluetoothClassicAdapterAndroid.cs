#define DEBUG_UNIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Util;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Adapters;
using SiamCross.Models.Tools;
using OperationCanceledException = System.OperationCanceledException;

namespace SiamCross.Droid.Models
{
    public class BaseBluetoothClassicAdapterAndroid : BroadcastReceiver, IBluetoothClassicAdapter
    {

        readonly IPhyInterface mInterface;
        public IPhyInterface PhyInterface
        {
            get => mInterface;
        }

        private BluetoothDevice _bluetoothDevice;
        private BluetoothSocket _socket;
        private BufferedReader _reader;
        private InputStreamReader _inputStreamReader;
        protected ScannedDeviceInfo _scannedDeviceInfo;

        protected Stream _outStream;
        protected Stream _inStream;

        protected Task _readTask;

        private const string _uuid = "00001101-0000-1000-8000-00805f9b34fb";

        public BaseBluetoothClassicAdapterAndroid(ScannedDeviceInfo deviceInfo)
        {
            _scannedDeviceInfo = deviceInfo;
            _reader = null;
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }

        private BluetoothAdapter _bluetoothAdapter;
        virtual public Task<byte[]> Exchange(byte[] req)
        {
            return null;
        }
        virtual public async Task<bool> Connect()
        {
            if (_scannedDeviceInfo.BluetoothArgs is string address)
            {
                _bluetoothDevice = _bluetoothAdapter.GetRemoteDevice(address);
                            }
            if (_bluetoothDevice == null) 
                return false;
            try
            {               
                _socket = _bluetoothDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(_uuid));

                if (_socket == null)
                {
                    System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect " 
                        + _scannedDeviceInfo.Name + ": _socket was null!");
                    return false;
                }

                if (!_socket.IsConnected)
                {
                    
                    await _socket.ConnectAsync();
                }

                _outStream = _socket.OutputStream;
                _inStream = _socket.InputStream;

                _inputStreamReader = new InputStreamReader(_inStream);
                _reader = new BufferedReader(_inputStreamReader);

                await Task.Delay(2000);

                _cancellToken = new CancellationTokenSource();
                _readTask = new Task(() => BackgroundRead(_cancellToken));
                _readTask.Start();

                ConnectSucceed?.Invoke();
                return true;
            }
            catch(Java.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect " 
                    + _scannedDeviceInfo.Name + ": "  + e.Message);
                await Disconnect();
            }
            catch(ObjectDisposedException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect "
                    + _scannedDeviceInfo.Name + ": " + e.Message);
                await Disconnect();
            }
            catch (Java.Lang.NullPointerException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect "
                    + _scannedDeviceInfo.Name + ": " + e.Message);
                await Disconnect();
            }
            return false;
        }

        public async Task Disconnect()
        {
            if(_cancellToken != null)
            {
                _cancellToken.Cancel();
            }
            
            Close(_socket);
            Close(_inStream);
            Close(_inputStreamReader);
            Close(_outStream);
        }

        private void Close(IDisposable connectedObject)
        {
            if (connectedObject == null)
            {
                return;
            }
            try
            {
                connectedObject?.Dispose();
            }
            catch (Exception)
            {
                Disconnect();
                throw;
            }

            connectedObject = null;
        }

        protected CancellationTokenSource _cancellToken;

        virtual protected async Task BackgroundRead(CancellationTokenSource _cancellToken)
        {
            while (!_cancellToken.IsCancellationRequested)
            {
                //_cancellToken.Token.ThrowIfCancellationRequested();
                if (!_inStream.CanRead || !_inStream.IsDataAvailable())
                {                  
                    continue;
                }
                byte[] inBuf = new byte[1];

                int readLen = _inStream.Read(inBuf, 0, inBuf.Length);
                DataReceived?.Invoke(inBuf);
            }
        }

        virtual public async Task SendData(byte[] data)
        {
            try
            {
                await _outStream.WriteAsync(data);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapter.SendData  " + _scannedDeviceInfo.Name + ": " + e.Message);
                ConnectFailed?.Invoke();
            }
             await Task.Delay(Constants.ShortDelay);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            throw new NotImplementedException();
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;

        protected void DoActionDataReceived(byte[] data) 
        {
            DataReceived?.Invoke(data);
        }
        protected void DoActionConnectSucceed()
        {
            ConnectSucceed?.Invoke();
        }
        protected void DoActionConnectFailed()
        {
            ConnectFailed?.Invoke();
        }
    }

    public class BluetoothClassicAdapterAndroid : BaseBluetoothClassicAdapterAndroid
    {
        public BluetoothClassicAdapterAndroid(ScannedDeviceInfo deviceInfo)
            :base(deviceInfo)
        {
        }

        virtual public async Task<bool> Connect()
        {
            bool res = await base.Connect();
            _cancellToken?.Cancel();
            await _readTask;
            _inStream.ReadTimeout = mExchangeTimeout;
            return res ;
        }
        //private Memory<byte> mStreamBuf = new byte[512];
        private byte[] mStreamBuf = new byte[512];

        //private Object lockObj = new Object();
        //private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        //using (await semaphore.UseWaitAsync())
        //{
        //    Assert.Equal(0, semaphore.CurrentCount);
        //}

        #if DEBUG
            public const int mExchangeTimeout = 30000;
            public const int mResponseRetry = 10;
        #else
                public const int mExchangeTimeout = 3000;
                public const int mResponseRetry = 256;
        #endif
        public const int mExchangeRetry = 3;
        public const int mRequestRetry = 3;


        private TaskCompletionSource<bool> tcs;// = new TaskCompletionSource<byte[]>();
        DataBuffer mBuf = new DataBuffer();
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

        private async Task DoByteProcess(byte[] inputBytes)
        {
            //Debug.WriteLine("Received " + inputBytes.Length.ToString() +
            //    ": [" + BitConverter.ToString(inputBytes) + "]\n");
            TaskCompletionSource<bool> ref_tsc = null;
            LockLog("Add - TryLock");
            //using (await semaphore.UseWaitAsync()) //lock (lockObj)
            {
                LockLog("Add - Locked");
                mBuf.Append(inputBytes);
                ref_tsc = tcs;
                LockLog("Add - Unlock");
            }
            LockLog("Add - SetResult");
            ref_tsc?.SetResult(true);
            LockLog("Add - exit");
        }
        private async void TimeoutWaitResponse()
        {
            TaskCompletionSource<bool> ref_tsc = null;
            //using (await semaphore.UseWaitAsync())
                ref_tsc = tcs;
            ref_tsc?.SetResult(false);
        }
        private async Task<bool> RequestAsync(byte[] data, CancellationToken ct)
        {
            bool sent = false;
            for (int i = 0; i < mRequestRetry && !sent; ++i)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await _outStream.WriteAsync(data);
                    sent = true;
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
            return sent;
        }
        private async Task<byte[]> ResponseAsync(byte[] req, CancellationToken ct)
        {
            byte[] pkg = { };
            UInt32 len_before = 0;
            UInt32 len_after = 0;
            ct.Register(() => TimeoutWaitResponse());

            for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            {
                try
                {
                    //res = await _readCharacteristic.ReadAsync(ct);
                    ct.ThrowIfCancellationRequested();
                    //LockLog("Try RespExtr");
                    //using (await semaphore.UseWaitAsync())
                    {
                        //LockLog("Lock RespExtr");
                        pkg = mBuf.Extract();
                        len_before = len_after = mBuf.Length;
                        //LockLog("UNLock RespExtr");
                    }

                    if (0 == pkg.Length)
                    {
                        //LockLog("Try RespCreate");
                        ///using (await semaphore.UseWaitAsync())
                        {
                            //LockLog("Lock RespCreate");
                            tcs = new TaskCompletionSource<bool>();
                            len_after = mBuf.Length;
                            //LockLog("UNLock RespCreate");
                        }

                        int readed = await _inStream.ReadAsync(mStreamBuf, 0, mStreamBuf.Length, ct);
                        mBuf.Append(mStreamBuf, readed);
                        
                        /*
                        if (len_before == len_after)
                        {
                            LockLog("Start await tcs.Task.Status = " + tcs.Task.Status.ToString());
                            bool result = await tcs.Task;
                            LockLog(" End await tcs.Task = " + result.ToString());
                            if (!result)
                                ct.ThrowIfCancellationRequested();
                            //Task delay_task = Task.Delay(mExchangeTimeout, ct);
                            //await Task.WhenAny(tcs.Task, delay_task);
                            //bool result = tmp_tcs.Task.Result;

                        }

                        LockLog("Try RespClear");
                        using (await semaphore.UseWaitAsync())
                        {
                            LockLog("Lock RespClear");
                            tcs = null;
                            LockLog("UNLock RespClear");
                        }
                        */
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
                catch (Exception sendingEx)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Ошибка получения BLE : "
                    + BitConverter.ToString(pkg)
                    + " " + sendingEx.Message + " "
                    + sendingEx.GetType() + " "
                    + sendingEx.StackTrace + "\n");
                }
                finally
                {
                    tcs = null;
                }
            }
            //if (0 == res.Length)
            //    ConnectFailed?.Invoke();
            return pkg;
        }
        public async Task<byte[]> SingleExchangeAsync(byte[] req)
        {

            byte[] res = { };
            CancellationTokenSource ctSrc = new CancellationTokenSource(mExchangeTimeout);
            try
            {
                mBuf.Clear();
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
            catch (AggregateException e)
            {
                System.Diagnostics.Debug.WriteLine("Истек таймаут подключения "
                    + _scannedDeviceInfo.Name + ": " + e.Message);
                await Disconnect();
            }
            finally
            {
                ctSrc.Dispose();
            }
            return res;
        }
        public async Task<byte[]> ExchangeData(byte[] req)
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
        private TaskCompletionSource<bool> mExecTcs;

        
        protected override async Task BackgroundRead(CancellationTokenSource _cancellToken)
        {
            while (!_cancellToken.IsCancellationRequested)
            {
                if (!_inStream.CanRead || !_inStream.IsDataAvailable())
                {
                    continue;
                }
                byte[] inBuf = new byte[1];
                

                int readLen = _inStream.Read(inBuf, 0, inBuf.Length);
                await DoByteProcess(inBuf);
                //DoActionDataReceived(inBuf);//DataReceived?.Invoke(inBuf);
            }
        }
        override async public Task<byte[]> Exchange(byte[] req)
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
        
        override public async Task SendData(byte[] req)
        {
            byte[] ret = await Exchange(req);

            if (null != ret && 0 < ret.Length)
            {
                DoActionDataReceived(ret);
                //DataReceived?.Invoke(ret);
            }

        }
        


    }

}