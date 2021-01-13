//#define DEBUG_UNIT

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface.Bt2;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{

    class BluetoothGattCallbackExt : BluetoothGattCallback
    {
        readonly BaseBluetoothClassicAdapterAndroid mBt2Adapter = null;
        public BluetoothGattCallbackExt(BaseBluetoothClassicAdapterAndroid bt2adapter)
            :base()
        {
            mBt2Adapter = bt2adapter;
        }
        override public void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            Debug.WriteLine($"OnConnectionStateChange state={newState}");
            //super.onConnectionStateChange(gatt, status, newState);
        }
        override public void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, GattStatus status)
        {
            Debug.WriteLine("OnReadRemoteRssi ");
            
            if (status == GattStatus.Success)
            {
                mBt2Adapter.Rssi = rssi;
                //Log.d("BluetoothRssi", String.format("BluetoothGat ReadRssi[%d]", rssi));
            }
                 
        }
    };
    
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { 
          BluetoothAdapter.ActionStateChanged
        , BluetoothAdapter.ExtraState
        , BluetoothDevice.ActionFound
        , BluetoothDevice.ExtraRssi
        , BluetoothDevice.ActionAclConnected
        , BluetoothDevice.ActionAclDisconnected
        , BluetoothDevice.ActionAclDisconnectRequested
        , BluetoothDevice.ActionBondStateChanged
        , BluetoothDevice.ActionNameChanged
    })]
    public class BaseBluetoothClassicAdapterAndroid : BroadcastReceiver, IConnection
    {

        readonly IPhyInterface mInterface;
        public IPhyInterface PhyInterface
        {
            get => mInterface;
        }

        public void UpdateRssi()
        {
            
        }
        private int mRssi=0;
        public int Rssi { get => mRssi; set => mRssi=value; }

        private BluetoothDevice _bluetoothDevice;
        private BluetoothSocket _socket;
        protected ScannedDeviceInfo _scannedDeviceInfo;

        protected Stream _outStream;

        private Task mRxThread=null;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private TaskCompletionSource<bool> mRxTsc = null;
        public Stream mRxStream = new MemoryStream(512);
        CancellationTokenSource CtRxSource = null;


        private const string _uuid = "00001101-0000-1000-8000-00805f9b34fb";

        private BluetoothAdapter BluetoothAdapter
        {
            get
            {
                if (null == mInterface)
                    return null;
                var bt2_ifc = mInterface as DroidBt2Interface;
                //if (null == ble_ifc)
                //    return null;
                return bt2_ifc?.mAdapter;
            }
        }
        public BaseBluetoothClassicAdapterAndroid()
            :this(null)
        { }
        public BaseBluetoothClassicAdapterAndroid(IPhyInterface ifc)
        {
            if (null == ifc)
                mInterface = Factory.GetCurent();
            else
                mInterface = ifc;
        }
        public BaseBluetoothClassicAdapterAndroid(ScannedDeviceInfo deviceInfo, IPhyInterface ifc = null)
            : this(ifc)
        {
            SetDeviceInfo(deviceInfo);
        }

        private void SetDeviceInfo(ScannedDeviceInfo deviceInfo)
        {
            _scannedDeviceInfo = deviceInfo;
        }
        virtual public async Task<bool> Connect()
        {
            try
            {
                if (null == mInterface)
                   return false;
                if (!mInterface.IsEnbaled)
                    mInterface.Enable();
                if (!mInterface.IsEnbaled)
                    return false;

                _bluetoothDevice = BluetoothAdapter.GetRemoteDevice(_scannedDeviceInfo.Mac);
                if (null == _bluetoothDevice)
                    return false;

                if (_bluetoothDevice.Name != _scannedDeviceInfo.Name)
                {
                    _bluetoothDevice.Dispose();
                    _bluetoothDevice = null;
                    return false;
                }

                //Context ctx = Android.App.Application.Context;
                //var callback = new BluetoothGattCallbackExt(this);
                //_bluetoothDevice.ConnectGatt(ctx, true, callback);


                //_socket = _bluetoothDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(_uuid));
                _socket = _bluetoothDevice.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(_uuid));

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

                CtRxSource = new CancellationTokenSource();
                mRxThread = Task.Factory.StartNew(
                    () => RxThreadFunction(_socket.InputStream, CtRxSource.Token)
                    , CtRxSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                DoActionConnectSucceed();
                return true;
            }
            catch(Java.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect " 
                    + _scannedDeviceInfo.Name + ": "  + e.Message);
                Disconnect();
            }
            catch(ObjectDisposedException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect "
                    + _scannedDeviceInfo.Name + ": " + e.Message);
                Disconnect();
            }
            catch (Java.Lang.NullPointerException e)
            {
                System.Diagnostics.Debug.WriteLine("BluetoothClassicAdapterMobile.Connect "
                    + _scannedDeviceInfo.Name + ": " + e.Message);
                Disconnect();
            }
            return false;
        }
        public void Disconnect()
        {
            try
            {
                RxThreadCancel();
                _outStream?.Close();
                _bluetoothDevice?.Dispose();
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR unknown in Disconnect");
            }
            finally
            {
                _outStream = null;
                _bluetoothDevice = null;
            }
        }
        void RxThreadCancel()
        {
            try
            {
                CtRxSource?.Cancel();
                mRxTsc?.TrySetResult(false);
                _socket?.Close();
                //_socket.Dispose();
                CtRxSource?.Dispose();
                mRxThread?.Dispose();
            }
            catch (Java.IO.IOException e)
            {
                Debug.WriteLine("close of connect socket failed "+e.Message);
            }
            finally
            {
                CtRxSource = null;
                mRxTsc = null;
                mRxThread = null;
                _socket = null;
            }
        }
        async Task RxThreadFunction(Stream rx_stream, CancellationToken ct)
        {
            Debug.WriteLine("RxTask start");
            byte[] buffer = new byte[512];
            int bytes;
            // Keep listening to the InputStream while connected
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    bytes = await rx_stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    DebugLog.WriteLine($"RxThreadFunction readed={bytes}");
                    using (await semaphore.UseWaitAsync()) //lock (lockObj)
                    {
                        //DebugLog.WriteLine($"RxThreadFunction begin write {bytes} to mRxStream");
                        await mRxStream.WriteAsync(buffer, 0, bytes, ct);
                        mRxTsc?.TrySetResult(true);
                        //DebugLog.WriteLine($"RxThreadFunction end write {bytes} to mRxStream");
                    }
                }
                catch (Java.IO.IOException e)
                {
                    Debug.WriteLine("RxTask canceled "+e.Message);
                    break;
                }
            }
            Debug.WriteLine("RxTask end");
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
                DoActionConnectFailed();
            }
             await Task.Delay(Constants.ShortDelay);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent?.Action;
            BluetoothDevice device = (BluetoothDevice)intent?.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            string device_name = device?.Name;
            Debug.WriteLine($"OnReceive {device_name} action={action}");
            /*
             
            if (BluetoothDevice.ActionAclConnected.Equals(action))
            {
                return;
            }
            //if (   BluetoothDevice.ExtraRssi.Equals(action) || BluetoothDevice.ActionFound.Equals(action) )
            {
                mRssi = intent.GetShortExtra(BluetoothDevice.ExtraRssi, short.MinValue);
            }


            if (BluetoothDevice.ActionAclDisconnected.Equals(action))
            {
                if(null!= this._bluetoothDevice && _bluetoothDevice.Equals(device))
                    Disconnect();
            }
            */
            //throw new NotImplementedException();
        }

        protected void DoActionConnectSucceed()
        {
            //ConnectSucceed?.Invoke();
        }
        protected void DoActionConnectFailed()
        {
            //ConnectFailed?.Invoke();
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
            _outStream.Flush();
            //_outStream.Position = 0;
            //_inStream.SetLength(0);
        }
        public async  Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            //https://github.com/dotnet/runtime/issues/23736
            //https://github.com/dotnet/runtime/issues/24093
            //https://github.com/dotnet/runtime/issues/19867
            // тут какая-то бага в фреймфорке поток никак не реагирует на CancellationToken
            // будем убивать поток и заново его открывать
            int readed = 0;
            try
            {
                ct.Register(() =>
                {
                    mRxTsc?.TrySetException(new OperationCanceledException());
                    mRxTsc?.TrySetResult(false);
                });
                while (0 == readed)
                {
                    ct.ThrowIfCancellationRequested();
                    //DebugLog.WriteLine("ReadAsync - Try Create");
                    using (await semaphore.UseWaitAsync())
                    {
                        //Debug.WriteLine("ReadAsync - Lock Create");
                        mRxStream.Position = 0;
                        readed = await mRxStream.ReadAsync(buffer, offset, count, ct);
                        DebugLog.WriteLine($"ReadAsync - readed = {readed}");
                        mRxStream.SetLength(0);
                    }
                    if (0 == readed)
                    {
                        DebugLog.WriteLine("ReadAsync - Begin Wait TSC");
                        mRxTsc = new TaskCompletionSource<bool>();
                        bool result = await mRxTsc?.Task;
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
                if (null != mRxTsc)
                {
                    using (await semaphore.UseWaitAsync())
                        mRxTsc = null;
                }
                */
            }
            DebugLog.WriteLine($"ReadAsync - return {readed}");
            return readed;
        }

        public async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await _outStream.WriteAsync(buffer, offset, count, ct);
            return count;
        }

    }

    public class BluetoothClassicAdapterAndroid : SiamProtocolConnection, IConnectionBt2
    {
        public BluetoothClassicAdapterAndroid(ScannedDeviceInfo deviceInfo)
         :base(new BaseBluetoothClassicAdapterAndroid(deviceInfo))
        {
        }

        public override async Task<bool> Connect()
        {
            if (mBaseConn is BaseBluetoothClassicAdapterAndroid)
            {
                bool res = await base.Connect();
                if (res)
                {
                    DoActionConnectSucceed();
                    return res;
                }
            }
            DoActionConnectFailed();
            return false;
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