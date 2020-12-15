//#define DEBUG_UNIT

using Autofac;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using OperationCanceledException = System.OperationCanceledException;
using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;


//[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    public class BaseBluetoothLeAdapterAndroid : IConnection
    {
        readonly IPhyInterface mInterface;
        public IPhyInterface PhyInterface
        {
            get => mInterface;
        }
        public async void UpdateRssi()
        {
            bool succ = await _device?.UpdateRssiAsync();
        }
        public int Rssi { get => (null == _device) ? 0 : _device.Rssi; }
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

        private bool _isFirstConnectionTry = true;

        public BaseBluetoothLeAdapterAndroid(IPhyInterface ifc)
        {
            if (null == ifc)
                mInterface = BtLeInterface.Factory.GetCurent();
            else
                mInterface = ifc;
        }


        public BaseBluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo, IPhyInterface ifc = null)
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
            try
            {
                if (null == mInterface)
                    return false;
                if (!mInterface.IsEnbaled)
                    mInterface.Enable();
                if (!mInterface.IsEnbaled)
                    return false;

                if (_adapter == null)
                    return false;

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
                Disconnect();
                _isFirstConnectionTry = false;
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                Disconnect();
                _isFirstConnectionTry = false;
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
                Disconnect();
                _isFirstConnectionTry = false;
                return false;
            }

            if (_device == null)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                    + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                //ConnectFailed();
                Disconnect();
                _isFirstConnectionTry = false;
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

                _adapter.DeviceConnectionLost += (o, args) =>
                {
                    if (args.Device  == _device)
                    {
                        Debug.WriteLine("DeviceConnectionLost " + args.ErrorMessage);
                        Disconnect();
                    }
                };


                //ConnectSucceed?.Invoke();
                inited = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect "
                    + _deviceInfo.Name + " ошибка инициализации: " + e.Message);
                Disconnect();
                _isFirstConnectionTry = false;
            }

            return inited;
        }

        public async Task SendData_Old(byte[] data)
        {
            if (!mInterface.IsEnbaled)
            {
                //ConnectFailed?.Invoke();
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
                    catch (Exception e)
                    {
                        Debug.WriteLine("UNKNOWN exception in "
                        + $"Ошибка повторной попытки отправки номер {i}/3 сообщения BLE: " + "\n"
                        + System.Reflection.MethodBase.GetCurrentMethod().Name
                        + " : " + e.Message );
                    }
                }
                // Возможно нужно сделать дисконект
                //ConnectFailed?.Invoke();
            }
        }

        public void Disconnect()
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
                    Debug.WriteLine("UNKNOWN exception in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + " : " + e.Message);
                }
            };

            await _adapter.StartScanningForDevicesAsync();

            return await idevice.Task;
        }

        //public event Action<byte[]> DataReceived;
        //public event Action ConnectSucceed;
        //public event Action ConnectFailed;

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
            //DebugLog.WriteLine(ret);
        }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private TaskCompletionSource<bool> tcs;// = new TaskCompletionSource<byte[]>();
        private Stream mRxStream = new MemoryStream(512);


        private void TimeoutWaitResponse(TaskCompletionSource<bool> ref_tsc)
        {
            //TaskCompletionSource<bool> ref_tsc = null;
            //using (await semaphore.UseWaitAsync())
            //    ref_tsc = tcs;
            ref_tsc?.SetResult(false);
        }
        public async void ClearRx()
        {
            using (await semaphore.UseWaitAsync())
            {
                mRxStream.Flush();
                mRxStream.Position = 0;
                mRxStream.SetLength(0);
            }
        }
        public void ClearTx()
        {
        }
        private async void DoByteProcess(byte[] inputBytes)
        {
            LockLog("Add - Try");
            using (await semaphore.UseWaitAsync()) //lock (lockObj)
            {
                LockLog("Add - Lock");
                await mRxStream.WriteAsync(inputBytes);
                LockLog("Add - SetResult");
                tcs?.TrySetResult(true);
            }
        }
        private async Task<int> DoReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int readed = 0;
            try
            {
                ct.Register(() =>
                {
                    tcs?.TrySetException(new OperationCanceledException());
                    tcs?.TrySetResult(false);
                });
                while (0 == readed)
                {
                    ct.ThrowIfCancellationRequested();
                    LockLog("Read - Try Create");
                    using (await semaphore.UseWaitAsync())
                    {
                        LockLog("Read - Lock Create");
                        mRxStream.Position = 0;
                        readed = await mRxStream.ReadAsync(buffer, offset, count, ct);
                        mRxStream.SetLength(0);
                    }
                    if (0 == readed)
                    {
                        LockLog("Read - Begin Wait TSC");
                        tcs = new TaskCompletionSource<bool>();
                        bool result = await tcs?.Task;
                        if (!result)
                            ct.ThrowIfCancellationRequested();
                    }
                }//while (0 == readed)
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                /*
                if (null != tcs)
                {
                    using (await semaphore.UseWaitAsync())
                        tcs = null;
                }
                */
            }
            return readed;
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int readed = 0;
            try
            {
                readed = await DoReadAsync(buffer, offset, count, ct);
            }
            catch (OperationCanceledException ex)
            {
                //DebugLog.WriteLine("ReadAsync canceled by timeout {0}: {1}"
                //    , ex.GetType().Name, ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("UNKNOWN exception in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                throw ex;
            }
            finally
            {

            }
            return readed;
        }
        public async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            bool sent = await _writeCharacteristic
                .WriteAsync(buffer.AsSpan().Slice(offset, count).ToArray(), ct);
            if(!sent)
                return 0;
            return count;
        }
    }

    public class BluetoothLeAdapterAndroid : SiamProtocolConnection, IConnectionBtLe
    {
        public BluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo, IPhyInterface ifc = null)
            : base(new BaseBluetoothLeAdapterAndroid(deviceInfo, ifc))
        {
        }
        public BluetoothLeAdapterAndroid(ScannedDeviceInfo deviceInfo)
         : base(new BaseBluetoothLeAdapterAndroid(deviceInfo))
        {
        }
        public override void DoActionDataReceived(byte[] data)
        {
            DataReceived?.Invoke(data);
        }
        public override void DoActionConnectSucceed()
        {
            ConnectSucceed?.Invoke();
        }
        public override void DoActionConnectFailed()
        {
            ConnectFailed?.Invoke();
        }

        public override event Action<byte[]> DataReceived;
        public override event Action ConnectSucceed;
        public override event Action ConnectFailed;
    }
}