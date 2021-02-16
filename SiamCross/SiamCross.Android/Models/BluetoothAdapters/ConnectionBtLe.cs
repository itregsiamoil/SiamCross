//#define DEBUG_UNIT

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using SiamCross.Models.Adapters;
using SiamCross.Models.Connection;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using OperationCanceledException = System.OperationCanceledException;

//[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    public class ConnectionBtLe : IConnectionBtLe
    {
        private readonly IPhyInterface mInterface;
        public override IPhyInterface PhyInterface => mInterface;

        private int _Rssi = 0;
        public override int Rssi => _Rssi;
        public override async void UpdateRssi()
        {
            try
            {
                if (null != _device)
                {
                    await _device.UpdateRssiAsync();
                    _Rssi = _device.Rssi;
                }
                else
                    _Rssi = 0;
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("EXCEPTION in UpdateRssi"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                _Rssi = 0;
            }
            OnPropChange(new PropertyChangedEventArgs(nameof(Rssi)));
        }
        public IAdapter Adapter
        {
            get
            {
                if (null == mInterface)
                    return null;
                BtLeInterface ble_ifc = mInterface as BtLeInterface;
                //if (null == ble_ifc)
                //    return null;
                return ble_ifc?.Adapter;
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

        public ConnectionBtLe(IPhyInterface ifc)
        {
            if (null == ifc)
                mInterface = BtLeInterface.Factory.GetCurent();
            else
                mInterface = ifc;
        }


        public ConnectionBtLe(ScannedDeviceInfo deviceInfo, IPhyInterface ifc = null)
            : this(ifc)
        {
            SetDeviceInfo(deviceInfo);
        }

        public void SetDeviceInfo(ScannedDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
            _deviceGuid = (Guid)deviceInfo.Id;
        }

        public override async Task<bool> Connect()
        {
            SetState(ConnectionState.PendingConnect);
            bool ret = await DoConnect();
            if (ret)
                SetState(ConnectionState.Connected);
            else
                SetState(ConnectionState.Disconnected);
            return ret;
        }
        public override async Task<bool> Disconnect()
        {
            SetState(ConnectionState.PendingDisconnect);
            bool ret = await DoDisconnect();
            if (ret)
                SetState(ConnectionState.Disconnected);
            return ret;
        }

        private async Task<bool> DoConnect()
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
                    await Disconnect();
                    //_isFirstConnectionTry = false;
                    return false;
                }
                bool is_inited = await Initialize(cts.Token);
                if (!is_inited)
                    await Disconnect();
                else
                    return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.Name + ": " + e.Message);
                await Disconnect();
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

                IReadOnlyList<ICharacteristic> serv = await _targetService.GetCharacteristicsAsync();
                _writeCharacteristic = await _targetService.GetCharacteristicAsync(write_guid);
                _readCharacteristic = await _targetService.GetCharacteristicAsync(read_guid);
                _readCharacteristic.ValueUpdated += (o, args) =>
                {
                    //Debug.WriteLine("Recieved: " + BitConverter.ToString(args.Characteristic.Value) + "\n");
                    //DataReceived?.Invoke(args.Characteristic.Value);
                    if (args.Characteristic == _readCharacteristic)
                    {
                        //mDataNotyfy?.Invoke(args.Characteristic.Value);
                        DoByteProcess(args.Characteristic.Value);
                    }
                };
                //tcs = new TaskCompletionSource<byte[]>();
                //mDataNotyfy += DoByteProcess;

                await _readCharacteristic.StartUpdatesAsync();
                //_isFirstConnectionTry = true;

                Adapter.DeviceConnectionLost += async (o, args) =>
                {
                    if (args.Device == _device)
                    {
                        Debug.WriteLine("DeviceConnectionLost " + args.ErrorMessage);
                        await Disconnect();
                    }
                };


                //ConnectSucceed?.Invoke();
                inited = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect "
                    + _deviceInfo.Name + " ошибка инициализации: " + e.Message);
                await Disconnect();
                //_isFirstConnectionTry = false;
            }

            return inited;
        }
        private async Task<bool> DoDisconnect()
        {
            bool ret = true;
            try
            {
                if (null != Adapter && null != _device)
                    await Adapter.DisconnectDeviceAsync(_device);
                await Task.Delay(200);

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
                ret = false;
            }
            finally
            {
                _writeCharacteristic = null;
                _readCharacteristic = null;
                _device = null;
                _targetService = null;
            }
            return ret;
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

        public override async void ClearRx()
        {
            using (await semaphore.UseWaitAsync())
            {
                mRxStream.Flush();
                mRxStream.Position = 0;
                mRxStream.SetLength(0);
            }
        }
        public override void ClearTx()
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

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
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
        public override async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            bool sent = await _writeCharacteristic
                .WriteAsync(buffer.AsSpan().Slice(offset, count).ToArray(), ct);
            if (!sent)
                return 0;
            return count;
        }
    }

}