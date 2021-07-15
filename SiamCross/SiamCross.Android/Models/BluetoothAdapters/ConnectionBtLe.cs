//#define DEBUG_UNIT

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SiamCross.Droid.Models.BluetoothAdapters;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection;
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

//[assembly: Dependency(typeof(BluetoothLeAdapterAndroid))]
namespace SiamCross.Droid.Models
{
    [Fody.ConfigureAwait(false)]
    public class ConnectionBtLe : IConnectionBtLe
    {
        private async void OnDisconected(object obj, DeviceErrorEventArgs args)
        {
            if (args.Device == _device)
            {
                Debug.WriteLine("DeviceConnectionLost " + args.ErrorMessage);
                await Disconnect();
            }
        }
        private async void OnReceiveData(object obj, CharacteristicUpdatedEventArgs args)
        {
            //Debug.WriteLine("Recieved: " + BitConverter.ToString(args.Characteristic.Value) + "\n");
            //DataReceived?.Invoke(args.Characteristic.Value);
            if (args.Characteristic == _readCharacteristic)
            {
                //mDataNotyfy?.Invoke(args.Characteristic.Value);
                try
                {
                    if (null != CtRxSource)
                        await DoByteProcess(args.Characteristic.Value, CtRxSource.Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ERROR"
                        + System.Reflection.MethodBase.GetCurrentMethod().Name
                        + "\n msg=" + ex.Message
                        + "\n type=" + ex.GetType()
                        + "\n stack=" + ex.StackTrace + "\n");
                }

            }
        }

        private readonly IPhyInterface mInterface;
        public override IPhyInterface PhyInterface => mInterface;

        private int _Rssi = 0;
        public override int Rssi => _Rssi;
        public override async Task UpdateRssi()
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
            ChangeNotify(nameof(Rssi));
        }
        public IAdapter Adapter
        {
            get
            {
                if (null == mInterface)
                    return null;
                BtLeInterfaceDroid ble_ifc = mInterface as BtLeInterfaceDroid;
                //if (null == ble_ifc)
                //    return null;
                return ble_ifc?.Adapter;
            }
        }

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

        private ConnectionInterval _ConnInterval = ConnectionInterval.Normal;
        public int ConnInterval => _ConnInterval switch
        {
            ConnectionInterval.High => 20,
            ConnectionInterval.Low => 100,
            _ => 50,
        };

        private int _Mtu = 20;
        public override int Mtu => _Mtu;

        public ConnectionBtLe(IPhyInterface ifc)
        {
            if (null == ifc)
                mInterface = FactoryBtLe.GetCurent();
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
            _deviceGuid = Guid.Parse(deviceInfo.Id);
        }

        public override async Task<bool> Connect(CancellationToken ct)
        {
            using var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout);
            using var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct);
            using var slock = await semaphore.UseWaitAsync(linkTsc.Token);
            if (State == ConnectionState.Connected)
                return true;
            SetState(ConnectionState.PendingConnect);
            bool ret = await DoConnectAsync(linkTsc.Token);
            if (ret)
                SetState(ConnectionState.Connected);
            else
                SetState(ConnectionState.Disconnected);
            return ret;
        }
        public override async Task<bool> Disconnect()
        {
            using var slock = await semaphore.UseWaitAsync(new CancellationTokenSource().Token);
            SetState(ConnectionState.PendingDisconnect);
            bool ret = await DoDisconnectAsync();
            if (ret)
                SetState(ConnectionState.Disconnected);
            return ret;
        }

        private async Task<bool> DoConnectAsync(CancellationToken ct)
        {
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
                    await Adapter.ConnectToDeviceAsync(_device, conn_param, ct);
                }


                // try get NON paired
                if (null == _device)
                {
                    _device = await Adapter.ConnectToKnownDeviceAsync(_deviceGuid, conn_param, ct);
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
                        + _deviceInfo.PhyName + "ошибка соединения BLE - _device был null");
                    //ConnectFailed();
                    await DoDisconnectAsync();
                    //_isFirstConnectionTry = false;
                    return false;
                }
                bool is_inited = await InitializeAsync(ct);
                if (!is_inited)
                    await DoDisconnectAsync();
                else
                    return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine("BluetoothLeAdapterMobile.Connect ошибка подключения по Guid "
                    + _deviceInfo.PhyName + ": " + e.Message);
                await DoDisconnectAsync();
                //_isFirstConnectionTry = false;
            }
            finally
            {
            }
            return false;
        }

        private async Task<bool> InitializeAsync(CancellationToken ct)
        {
            bool inited = false;
            try
            {
                _Mtu = await _device.RequestMtuAsync(Constants.BTLE_DEFAULT_MTU) - Constants.BTLE_PKG_HDR_SIZE;
                ChangeNotify(nameof(Mtu));

                if (_device.UpdateConnectionInterval(ConnectionInterval.High))
                    _ConnInterval = ConnectionInterval.High;
                else
                    _ConnInterval = ConnectionInterval.Normal;
                ChangeNotify(nameof(ConnInterval));

                _targetService = await _device.GetServiceAsync(svc_guid, ct);
                //IReadOnlyList<IService> svc = await _device.GetServicesAsync(ct);
                if (null == _targetService)
                    return false;

                IReadOnlyList<ICharacteristic> serv = await _targetService.GetCharacteristicsAsync();
                _writeCharacteristic = await _targetService.GetCharacteristicAsync(write_guid);
                _readCharacteristic = await _targetService.GetCharacteristicAsync(read_guid);
                /*
                try 
                {
                    _writeCharacteristic.WriteType = CharacteristicWriteType.WithoutResponse;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"_deviceInfo.PhyName {ex.Message}");
                    _writeCharacteristic = await _targetService.GetCharacteristicAsync(write_guid);
                    _readCharacteristic = await _targetService.GetCharacteristicAsync(read_guid);
                }
                */

                CtRxSource = new CancellationTokenSource();
                _readCharacteristic.ValueUpdated += OnReceiveData;
                //tcs = new TaskCompletionSource<byte[]>();
                //mDataNotyfy += DoByteProcess;
                await _readCharacteristic.StartUpdatesAsync();
                //_isFirstConnectionTry = true;

                Adapter.DeviceConnectionLost += OnDisconected;
                ct.ThrowIfCancellationRequested();
                //ConnectSucceed?.Invoke();
                inited = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothLeAdapterMobile.Connect "
                    + _deviceInfo.PhyName + " ошибка инициализации: " + e.Message);
                await DoDisconnectAsync();
                //_isFirstConnectionTry = false;
            }

            return inited;
        }
        private async Task<bool> DoDisconnectAsync()
        {
            bool ret = true;
            try
            {
                if (null != _readCharacteristic)
                {
                    await _readCharacteristic.StopUpdatesAsync();
                    _readCharacteristic.ValueUpdated -= OnReceiveData;
                }
                _targetService?.Dispose();

                if (null != Adapter)
                {
                    Adapter.DeviceConnectionLost -= OnDisconected;
                    if (null != _device)
                    {
                        await Adapter.DisconnectDeviceAsync(_device);
                        _device.Dispose();
                    }
                }
                CtRxSource?.Cancel();
                await Task.Delay(200);
                CtRxSource?.Dispose();
                //mInterface?.Disable();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR"
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
                CtRxSource = null;
            }
            return ret;
        }

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private TaskCompletionSource<bool> tcs;// = new TaskCompletionSource<byte[]>();
        CancellationTokenSource CtRxSource;
        private readonly Stream mRxStream = new MemoryStream(Constants.MAX_PKG_SIZE * 2);

        public override async Task ClearRx()
        {
            CheckConnection();
            using var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout);
            using var slock = await semaphore.UseWaitAsync(ctSrc.Token);
            mRxStream.Flush();
            mRxStream.Position = 0;
            mRxStream.SetLength(0);
        }
        public override Task ClearTx()
        {
            CheckConnection();
            return Task.CompletedTask;
        }
        private async Task DoByteProcess(byte[] inputBytes, CancellationToken ct)
        {
            using (await semaphore.UseWaitAsync(ct)) //lock (lockObj)
            {
                await mRxStream.WriteAsync(inputBytes, 0, inputBytes.Length, ct);
                tcs?.TrySetResult(true);
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            CheckConnection();
            int readed = 0;
            ct.Register(() =>
            {
                tcs?.TrySetException(new OperationCanceledException());
                tcs?.TrySetResult(false);
            });
            while (0 == readed)
            {
                ct.ThrowIfCancellationRequested();
                using (await semaphore.UseWaitAsync(ct))
                {
                    mRxStream.Position = 0;
                    readed = await mRxStream.ReadAsync(buffer, offset, count, ct);
                    mRxStream.SetLength(0);
                }
                if (0 == readed)
                {
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
            CheckConnection();
            using var slock = await semaphore.UseWaitAsync(ct);
            int sent = 0;
            int curr_count;
            while (sent < count)
            {
                ct.ThrowIfCancellationRequested();
                curr_count = (sent + Mtu > count) ? (count - sent) : Mtu;
                byte[] buf = buffer.AsSpan().Slice(offset + sent, curr_count).ToArray();
                bool is_ok = await _writeCharacteristic.WriteAsync(buf, ct);
                DebugLog.WriteLine($" writing chunk size={curr_count} - resilt is {is_ok}");
                if (!is_ok)
                {
                    if (Mtu > 20)
                    {
                        Debug.WriteLine($"Set minimum Mtu=20");
                        _Mtu = await _device.RequestMtuAsync(20 + 3) - 3;
                        ChangeNotify(nameof(Mtu));
                    }
                    return 0;
                }
                sent += curr_count;
            }
            return sent;
        }
        public override async void Dispose()
        {
            await Disconnect();
            mInterface.Disable();
        }
    }

}