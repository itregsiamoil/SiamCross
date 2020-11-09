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
using Plugin.BLE.Abstractions;
using Android.Bluetooth;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;
using Debug = System.Diagnostics.Debug;
using OperationCanceledException = System.OperationCanceledException;


[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    public class BluetoothLeAdapterAndroid : IBluetoothLeAdapter
    {
        private IAdapter _adapter;
        private IDevice _device;
        private Guid _deviceGuid;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private static string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167c";
        private static string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167c";
        private static string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private ScannedDeviceInfo _deviceInfo;

        private List<string> _connectQueue;

        private bool _isFirstConnectionTry = true;

        public BluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo)
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _deviceInfo = deviceInfo;
            _deviceGuid = (Guid)deviceInfo.BluetoothArgs;
            

            if (_connectQueue == null)
            {
                _connectQueue = new List<string>();
            }
        }

        public async Task<bool> Connect()
        {
            bool connected = false;
            if(!BluetoothAdapter.DefaultAdapter.IsEnabled)
                return connected;
            if (_connectQueue.Contains(_deviceInfo.Name))
                return connected;
            else
            {
                _connectQueue.Add(_deviceInfo.Name);
            }

            if (_adapter == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения" +
                    " _adapter == null. Будет произведена переинициализация адаптера");
                await Disconnect();
                _adapter = CrossBluetoothLE.Current.Adapter;
                _connectQueue.Remove(_deviceInfo.Name);
                return connected;
            }

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
                return connected;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return connected;
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
                _adapter = CrossBluetoothLE.Current.Adapter;
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return connected;
            }

            if (_device == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                    + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                ConnectFailed();
                await Disconnect();
                _isFirstConnectionTry = false;
                _connectQueue.Remove(_deviceInfo.Name);
                return connected;
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

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public const int mExchangeTimeout = 3000;
        public const int mExchangeRetry = 3;
        public const int mRequestRetry = 3;
        public const int mResponseRetry = 256;

        private TaskCompletionSource<byte[]> tcs;// = new TaskCompletionSource<byte[]>();
        DataBuffer mBuf = new DataBuffer();

        private void DoByteProcess(byte[] inputBytes)
        {
            //Debug.WriteLine("Received " + inputBytes.Length.ToString() +
            //    ": [" + BitConverter.ToString(inputBytes) + "]\n");
            mBuf.Append(inputBytes);
            if (null != tcs)
                tcs.TrySetResult(inputBytes);
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
            byte[] res = { };
            
            for (int i = 0; i < mResponseRetry && 0 == pkg.Length; ++i)
            {
                try
                {
                    //res = await _readCharacteristic.ReadAsync(ct);
                    ct.ThrowIfCancellationRequested();
                    var len = mBuf.Length;
                    pkg = mBuf.Extract();
                    if(0==pkg.Length)
                    {
                        tcs = new TaskCompletionSource<byte[]>();
                        using (ct.Register(() =>
                        {
                            // this callback will be executed when token is cancelled
                            tcs.TrySetCanceled();
                        })) 

                        if (mBuf.Length == len)
                            await tcs.Task;
                        tcs = null;
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
                    throw ex;
                }
                catch (Exception sendingEx)
                {
                    DebugLog.WriteLine("try " + i.ToString() + " - Ошибка получения BLE : "
                    + BitConverter.ToString(res)
                    + " " + sendingEx.Message + " "
                    + sendingEx.GetType() + " "
                    + sendingEx.StackTrace + "\n");
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
                DebugLog.WriteLine("Eaxchange canceled by timeout {0}: {1}", ex.GetType().Name, ex.Message);
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

        public async Task<byte[]> Exchange(byte[] req)
        {
            var curr_task = GetCurrent();
            if (null != curr_task)
            {
                DebugLog.WriteLine("WARNING another tas running");
                await curr_task;
                //await cur_task;
            }
                

            var task = ExchangeData(req);
            SetCurrent(task);
            byte[] ret = await task;
            task.Dispose();
            task = null;
            SetCurrent(task);
            return ret;
        }

        private Object mLock = new Object();
        private Task mCurrenTask;
        public Task GetCurrent()
        {
            lock(mLock)
            {
                return mCurrenTask;
            }
        }
        public void SetCurrent(Task task)
        {
            lock (mLock)
            {
                mCurrenTask=task;
            }
        }

        public async Task SendData(byte[] data)
        {
            var res = await Exchange(data);

            if (null != res && 0 < res.Length)
                DataReceived?.Invoke(res);
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
                _adapter = null;

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