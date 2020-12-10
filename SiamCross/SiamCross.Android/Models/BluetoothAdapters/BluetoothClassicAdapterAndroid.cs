#define DEBUG_UNIT

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Scanners;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Droid.Models
{
    
    class BluetoothGattCallbackExt : BluetoothGattCallback
    {
        BaseBluetoothClassicAdapterAndroid mBt2Adapter = null;
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

        public async void UpdateRssi()
        {
            
        }
        private int mRssi=0;
        public int Rssi { get => mRssi; set => mRssi=value; }

        private BluetoothDevice _bluetoothDevice;
        private BluetoothSocket _socket;
        protected ScannedDeviceInfo _scannedDeviceInfo;

        protected Stream _outStream;
        public Stream _inStream;


        private const string _uuid = "00001101-0000-1000-8000-00805f9b34fb";

        private BluetoothAdapter _bluetoothAdapter
        {
            get
            {
                if (null == mInterface)
                    return null;
                var bt2_ifc = mInterface as Bt2Interface;
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
                mInterface = Bt2Interface.Factory.GetCurent();
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
            if (_scannedDeviceInfo.BluetoothArgs is string address)
            {
                _bluetoothDevice = _bluetoothAdapter.GetRemoteDevice(address);
                if (_bluetoothDevice.Name != _scannedDeviceInfo.Name)
                {
                    _bluetoothDevice.Dispose();
                    _bluetoothDevice = null;
                }
            }
            if (_bluetoothDevice == null) 
                return false;
            try
            {
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
                _inStream = _socket.InputStream;
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
                _inStream?.Close();
                _outStream?.Close();
                _socket?.Close();
                _bluetoothDevice?.Dispose();
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR unknown in Disconnect");
            }
            finally
            {
                _inStream = null;
                _outStream = null;
                _socket = null;
                _bluetoothDevice = null;
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
                DoActionConnectFailed();
            }
             await Task.Delay(Constants.ShortDelay);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            string device_name = device.Name;
            System.Diagnostics.Debug.WriteLine($"OnReceive action={action}");

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

        //public event Action<byte[]> DataReceived;
        //public event Action ConnectSucceed;
        //public event Action ConnectFailed;

        protected void DoActionDataReceived(byte[] data) 
        {
            //DataReceived?.Invoke(data);
        }
        protected void DoActionConnectSucceed()
        {
            //ConnectSucceed?.Invoke();
        }
        protected void DoActionConnectFailed()
        {
            //ConnectFailed?.Invoke();
        }

        public void ClearRx()
        {
            ThrowIfNoConnection();
            _inStream.Flush();
            //_inStream.Position = 0;
            //_inStream.SetLength(0);
        }
        public void ClearTx()
        {
            ThrowIfNoConnection();
            _outStream.Flush();
            //_outStream.Position = 0;
            //_inStream.SetLength(0);
        }
        void StreamDisconnect()
        {
            try
            {
                _inStream?.Close();
                _outStream?.Close();
                _socket?.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("StreamDisconnect ERROR unknown");
            }
            finally
            {
                _inStream = null;
                _outStream = null;
                _socket = null;
            }
        }
        void ThrowIfNoConnection()
        {
            if (null != _inStream && null != _outStream && null != _socket && _socket.IsConnected)
                return;
            throw new OperationCanceledException();
        }

        public async  Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            //https://github.com/dotnet/runtime/issues/23736
            //https://github.com/dotnet/runtime/issues/24093
            //https://github.com/dotnet/runtime/issues/19867
            // тут какая-то бага в фреймфорке поток никак не реагирует на CancellationToken
            // будем убивать поток и заново его открывать
            ct.ThrowIfCancellationRequested();
            bool soc_closed = false;
            try
            {
                ct.ThrowIfCancellationRequested();
                ThrowIfNoConnection();

                ct.Register(() =>
                {
                    Debug.WriteLine("ReadAsync cancel by timeout");
                    if (ct.IsCancellationRequested)
                    {
                        soc_closed = true;
                        StreamDisconnect();
                    }

                });
                return await _inStream.ReadAsync(buffer, offset, count, ct);
            }
            catch (Exception ex)
            {
                if (!soc_closed)
                {
                    Debug.WriteLine("ReadAsync unknown err - rethrow "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType());
                    throw ex;
                }
            }
            return 0;
        }



        public async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            ThrowIfNoConnection();
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
            var buggly_conn = mBaseConn as BaseBluetoothClassicAdapterAndroid;
            if(null!= buggly_conn)
            {
                bool res = await base.Connect();
                //buggly_conn._inStream.ReadTimeout = mExchangeTimeout;
                if(res)
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