#define DEBUG_UNIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;
using SiamCross.Droid.Models;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Adapters;
using SiamCross.Models.Tools;
using Xamarin.Forms;
using Plugin.BLE.Abstractions.EventArgs;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services.Logging;
using Android.Bluetooth;
using Debug = System.Diagnostics.Debug;
using OperationCanceledException = System.OperationCanceledException;


//[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    public class BluetoothLeAdapterAndroid : IConnectionBtLe
    {
        readonly IPhyInterface mInterface;
        public IPhyInterface PhyInterface 
        {
            get => mInterface;
        }

        public IAdapter _adapter
        {
            get 
            {
                if (null == mInterface)
                    return null;
                var ble_ifc = mInterface as BtLeInterface;
                //if (null == ble_ifc)
                //    return null;
                return ble_ifc?.mAdapter;
            }
        }

        private IDevice _device;
        private Guid _deviceGuid;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private static string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167c";
        private static string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167c";
        private static string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private ScannedDeviceInfo _deviceInfo;

        private List<string> _connectQueue = new List<string>();

        private bool _isFirstConnectionTry = true;

        public BluetoothLeAdapterAndroid(IPhyInterface ifc)
        {
            if (null == ifc)
                mInterface = BtLeInterface.Factory.GetCurent();
            else
                mInterface = ifc;
        }


        public BluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo, IPhyInterface ifc = null)
            : this(ifc)
        {
            SetDeviceInfo(deviceInfo);
        }

        public void SetDeviceInfo(ScannedDeviceInfo deviceInfo)
        {
           _deviceInfo = deviceInfo;
           _deviceGuid = (Guid)deviceInfo.BluetoothArgs;
        }

        public async Task<bool> Connect()
        {
            if (null == mInterface)
                return false;
            if (!mInterface.IsEnbaled)
                mInterface.Enable();

            if (_adapter == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения" +
                    " _adapter == null. Будет произведена переинициализация адаптера");
                await Disconnect();
                _connectQueue.Remove(_deviceInfo.Name);
                return false;
            }

            if (_connectQueue.Contains(_deviceInfo.Name))
                return true;
            else
                _connectQueue.Add(_deviceInfo.Name);



            try
            {
                if (_isFirstConnectionTry)
                {
                    await _adapter.ConnectToKnownDeviceAsync(_deviceGuid);
                }
                else
                {
                    IDevice dev = await CreateIDevice(_deviceGuid);
                    await _adapter.ConnectToDeviceAsync(dev);
                }
            }
            catch (AggregateException e)
            {
                System.Diagnostics.Debug.WriteLine("Истек таймаут подключения "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return false;
            }

            if (_adapter != null)
            {
                _device = _adapter.ConnectedDevices.Where(x => x.Id == _deviceGuid)
                    .LastOrDefault();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения" +
                    " _adapter == null. Будет произведена переинициализация адаптера");
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return false;
            }

            if (_device == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                    + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                ConnectFailed();
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return false;
            }
            return await Initialize();
        }

        private async Task<bool> Initialize()
        {
            bool inited = false;
            try
            {
                _targetService = await _device.GetServiceAsync(Guid.Parse(_serviceGuid));

                var serv = await _targetService.GetCharacteristicsAsync();
                foreach (var ch in serv)
                {
                    System.Diagnostics.Debug.WriteLine(ch.Id);
                }

                _writeCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_writeCharacteristicGuid));
                _readCharacteristic = await _targetService.GetCharacteristicAsync(Guid.Parse(_readCharacteristicGuid));
                _readCharacteristic.ValueUpdated += (o, args) =>
                {
                    //Debug.WriteLine("Recieved: " + BitConverter.ToString(args.Characteristic.Value) + "\n");
                    //DataReceived?.Invoke(args.Characteristic.Value);
                    if(args.Characteristic== _readCharacteristic)
                    {
                        //mDataNotyfy?.Invoke(args.Characteristic.Value);
                        DoByteProcess(args.Characteristic.Value);
                    }
                };
                //tcs = new TaskCompletionSource<byte[]>();
                //mDataNotyfy += DoByteProcess;

                await _readCharacteristic.StartUpdatesAsync();
                _isFirstConnectionTry = true;
                ConnectSucceed?.Invoke();
                _connectQueue.Remove(_deviceInfo.Name);
                inited = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect "
                    + _deviceInfo.Name + " ошибка инициализации: " + e.Message);
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
            }
            return inited;
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
        //private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        //private Object lockObj = new Object();
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        //using (await semaphore.UseWaitAsync())
        //{
        //    Assert.Equal(0, semaphore.CurrentCount);
        //}



        #if DEBUG
            public const int mExchangeTimeout = 5000;
            public const int mResponseRetry = 10;
        #else
            public const int mExchangeTimeout = 3000;
            public const int mResponseRetry = 256;
        #endif
        public const int mExchangeRetry = 3;
        public const int mRequestRetry = 3;
        

        private TaskCompletionSource<bool> tcs;// = new TaskCompletionSource<byte[]>();
        DataBuffer mBuf = new DataBuffer();

        private async void DoByteProcess(byte[] inputBytes)
        {
            //Debug.WriteLine("Received " + inputBytes.Length.ToString() +
            //    ": [" + BitConverter.ToString(inputBytes) + "]\n");
            TaskCompletionSource<bool> ref_tsc = null;
            LockLog("Add - TryLock");
            using (await semaphore.UseWaitAsync()) //lock (lockObj)
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
            using (await semaphore.UseWaitAsync())
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
                    sent = await _writeCharacteristic.WriteAsync(data, ct);
                    DebugLog.WriteLine("Sent " + data.Length.ToString() +
                        ": [" + BitConverter.ToString(data) + "]\n");
                }
                catch (Exception sendingEx)
                {
                    DebugLog.WriteLine("try " + i.ToString()+" - Ошибка отправки BLE : "
                   + BitConverter.ToString(data)
                   + " " + sendingEx.Message + " "
                   + sendingEx.GetType() + " "
                   + sendingEx.StackTrace + "\n");
                }
            }
            if (!sent)
                ConnectFailed?.Invoke();
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
                    LockLog("Try RespExtr");
                    using (await semaphore.UseWaitAsync())
                    {
                        LockLog("Lock RespExtr");
                        pkg = mBuf.Extract();
                        len_before = len_after = mBuf.Length;
                        LockLog("UNLock RespExtr");
                    }
                    
                    if (0==pkg.Length)
                    {
                        LockLog("Try RespCreate");
                        using (await semaphore.UseWaitAsync())
                        {
                            LockLog("Lock RespCreate");
                            tcs = new TaskCompletionSource<bool>();
                            len_after = mBuf.Length;
                            LockLog("UNLock RespCreate");
                        }

                        if (len_before == len_after)
                        {
                            LockLog("Start await tcs.Task.Status = "+ tcs.Task.Status.ToString());
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
                    }
                    else
                    {
                        int cmp = req.AsSpan().Slice(0, 12).SequenceCompareTo(pkg.AsSpan().Slice(0, 12));
                        if(0!=cmp)
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
                if(sent)
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
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
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
            for (int i = 0; i<mExchangeRetry && 0==res.Length; ++i)
            {
                DebugLog.WriteLine("START transaction, try "+i.ToString());
                res = await SingleExchangeAsync(req);
                DebugLog.WriteLine("END transaction, try " + i.ToString());
            }
            return res;
        }

        private TaskCompletionSource<bool> mExecTcs;
        public async Task<byte[]> Exchange(byte[] req)
        {
            Task<byte[]> task=null;
            byte[] ret={ };
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
                DataReceived?.Invoke(ret);
        }


        public async Task SendData_Old(byte[] data)
        {
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                ConnectFailed?.Invoke();
                return;
            }
            System.Diagnostics.Debug.WriteLine("Send: " + BitConverter.ToString(data) + "\n");
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(1000);
                await _writeCharacteristic.WriteAsync(data, cancellationTokenSource.Token);
            }
            catch (Exception sendingEx)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка отправки сообщения BLE: "
                    + BitConverter.ToString(data)
                    + " " + sendingEx.Message + " " 
                    + sendingEx.GetType() + " " 
                    + sendingEx.StackTrace + "\n");
                for (int i = 1; i < 4; i++)
                {
                    try
                    {
                        await Task.Delay(500);
                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(500);
                        await _writeCharacteristic.WriteAsync(data, cancellationTokenSource.Token);
                        System.Diagnostics.Debug.WriteLine(
                            $"Повторная попытка отправки номер {i}/3 прошла успешно!" + "\n");
                        return;
                    }
                    catch (Exception resendingEx)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Ошибка повторной попытки отправки номер {i}/3 сообщения BLE: " + "\n");
                    }
                }
                // Возможно нужно сделать дисконект
                ConnectFailed?.Invoke();
            }
        }

        public async Task Disconnect()
        {
            if (_deviceGuid != null)
            {
                _writeCharacteristic = null;
                _readCharacteristic = null;
                mInterface?.Disable();

                _device?.Dispose();

                _targetService?.Dispose();

                _device = null;
                _targetService = null;
            }
        }

        private async Task<IDevice> CreateIDevice(Guid bluetoothGuid)
        {
            TaskCompletionSource<IDevice> idevice = new TaskCompletionSource<IDevice>();
            _adapter.ScanTimeout = 5000;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.ScanTimeoutElapsed += (s, e) => { try { idevice.SetResult(null); } catch { }; };
            _adapter.DeviceDiscovered += (obj, a) =>
            {
                try
                {
                    if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                    {
                        return;
                    }

                    if (a.Device.Id.Equals(bluetoothGuid))
                    {
                        _adapter.StopScanningForDevicesAsync();
                        idevice.SetResult(a.Device);
                    }
                    System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
                }
                catch (Exception e)
                {
                }
            };

            await _adapter.StartScanningForDevicesAsync();

            return await idevice.Task;
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;///////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!
    }
}