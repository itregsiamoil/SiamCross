//#define DEBUG_UNIT

using Plugin.BLE.Abstractions;
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
            await _device?.UpdateRssiAsync();
        }
        public int Rssi { get => (null == _device) ? 0 : _device.Rssi; }
        public IAdapter Adapter
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

#if DEBUG
        private static readonly int mConnectTimeout = 100000;
#else
        private static readonly int mConnectTimeout = 8000;
#endif


        private IDevice _device;
        private Guid _deviceGuid;
        private IService _targetService;
        private ICharacteristic _writeCharacteristic;
        private ICharacteristic _readCharacteristic;

        private static readonly string _writeCharacteristicGuid = "569a2001-b87f-490c-92cb-11ba5ea5167c";
        private static readonly string _readCharacteristicGuid = "569a2000-b87f-490c-92cb-11ba5ea5167c";
        private static readonly string _serviceGuid = "569a1101-b87f-490c-92cb-11ba5ea5167c";
        private static readonly Guid write_guid = Guid.Parse(_writeCharacteristicGuid);
        private static readonly Guid read_guid = Guid.Parse(_readCharacteristicGuid);
        private static readonly Guid svc_guid = Guid.Parse(_serviceGuid);

        private ScannedDeviceInfo _deviceInfo;
        //private bool _isFirstConnectionTry = true;

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
           _deviceGuid = (Guid)deviceInfo.Id;
        }

        public async Task<bool> Connect()
        {
            CancellationTokenSource cts = new CancellationTokenSource(mConnectTimeout);
            try
            {
                if (null == mInterface)
                    return false;
                if (!mInterface.IsEnbaled)
                    mInterface.Enable();
                if (!mInterface.IsEnbaled)
                    return false;

                if (Adapter == null)
                    return false;

                //ConnectParameters(autoConnect = false, forceBleTransport = true);
                ConnectParameters conn_param = new ConnectParameters(false, true);

                // try get paired
                IReadOnlyList<IDevice> paired = Adapter.GetSystemConnectedOrPairedDevices();
                _device = paired.Where(x => x.Id == _deviceGuid).FirstOrDefault();
                if (null != _device)
                {
                    await Adapter.ConnectToDeviceAsync(_device, conn_param, cts.Token);
                }


                // try get NON paired
                if (null == _device)
                {
                    _device = await Adapter.ConnectToKnownDeviceAsync(_deviceGuid, conn_param, cts.Token);
                }
                
                /*
                if (_isFirstConnectionTry)
                {
                    _device = await Adapter.ConnectToKnownDeviceAsync(_deviceGuid, conn_param, cts.Token);
                }
                else
                {
                    IDevice dev = await CreateIDevice(_deviceGuid);
                    await Adapter.ConnectToDeviceAsync(dev, conn_param, cts.Token);
                }
                _device = Adapter.ConnectedDevices.Where(x => x.Id == _deviceGuid)
                    .LastOrDefault();
                */
            
                if (_device == null)
                {
                    Debug.WriteLine("BluetoothLeAdapterMobile.Connect"
                        + _deviceInfo.Name + "ошибка соединения BLE - _device был null");
                    //ConnectFailed();
                    Disconnect();
                    //_isFirstConnectionTry = false;
                    return false;
                }            
                bool is_inited = await Initialize(cts.Token);
                if (!is_inited)
                    Disconnect();
                else
                    return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                Disconnect();
                //_isFirstConnectionTry = false;
            }
            finally
            {
                cts?.Dispose();
            }
            return false;
        }

        private async Task<bool> Initialize(CancellationToken ct)
        {
            bool inited = false;
            try
            {
                _targetService = await _device.GetServiceAsync(svc_guid, ct);
                //IReadOnlyList<IService> svc = await _device.GetServicesAsync(ct);
                if (null == _targetService)
                    return false;

                var serv = await _targetService.GetCharacteristicsAsync();
                _writeCharacteristic = await _targetService.GetCharacteristicAsync(write_guid);
                _readCharacteristic = await _targetService.GetCharacteristicAsync(read_guid);
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
                //_isFirstConnectionTry = true;

                Adapter.DeviceConnectionLost += (o, args) =>
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
                //_isFirstConnectionTry = false;
            }

            return inited;
        }
        public void Disconnect()
        {
            try
            {
                if(null!=Adapter && null!= _device)
                    Adapter.DisconnectDeviceAsync(_device);
                mInterface?.Disable();
                //_writeCharacteristic.StopUpdatesAsync();
                //_readCharacteristic.StopUpdatesAsync();
                _targetService?.Dispose();
                _device?.Dispose();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("ERROR"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                _writeCharacteristic = null;
                _readCharacteristic = null;
                _device = null;
                _targetService = null;
            }
        }
        private async Task<IDevice> CreateIDevice(Guid bluetoothGuid)
        {
            TaskCompletionSource<IDevice> idevice = new TaskCompletionSource<IDevice>();
            Adapter.ScanTimeout = 5000;
            Adapter.ScanMode = ScanMode.Balanced;
            Adapter.ScanTimeoutElapsed += (s, e) => { try { idevice.TrySetResult(null); } catch { }; };
            Adapter.DeviceDiscovered += async (obj, a) => 
            {
                try
                {
                    if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                    {
                        return;
                    }

                    if (a.Device.Id.Equals(bluetoothGuid))
                    {
                        await Adapter.StopScanningForDevicesAsync();
                        idevice.TrySetResult(a.Device);
                    }
                    Debug.WriteLine("Finded device" + a.Device.Name);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("UNKNOWN exception in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + " : " + e.Message);
                }
            };

            await Adapter.StartScanningForDevicesAsync();

            return await idevice.Task;
        }

        //public event Action<byte[]> DataReceived;
        //public event Action ConnectSucceed;
        //public event Action ConnectFailed;

        private static void LockLog(string msg)
        {
            /*
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
            */
        }

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private TaskCompletionSource<bool> tcs;// = new TaskCompletionSource<byte[]>();
        private readonly Stream mRxStream = new MemoryStream(512);

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

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int readed = 0;
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