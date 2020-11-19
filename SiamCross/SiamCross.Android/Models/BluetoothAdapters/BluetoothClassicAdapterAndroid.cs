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
using SiamCross.Models.Adapters.PhyInterface;

namespace SiamCross.Droid.Models
{
    public class BaseBluetoothClassicAdapterAndroid : BroadcastReceiver, IConnection
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
        public Stream _inStream;

        public Task _readTask;

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
            _reader = null;
        }


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

                //await Task.Delay(2000);

                _cancellToken = new CancellationTokenSource();
                //_readTask = new Task(() => BackgroundRead(_cancellToken));
                //_readTask.Start();

                DoActionConnectSucceed();
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
            _cancellToken?.Cancel();

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

        public CancellationTokenSource _cancellToken;

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
                DoActionDataReceived(inBuf);
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
            throw new NotImplementedException();
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
            _inStream.Flush();
            //_inStream.Position = 0;
            //_inStream.SetLength(0);
        }
        public void ClearTx()
        {
            _outStream.Flush();
            //_outStream.Position = 0;
            //_inStream.SetLength(0);
        }
        public async  Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            return await _inStream.ReadAsync(buffer, offset, count, ct);
        }
        public async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
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
                buggly_conn._cancellToken?.Cancel();
                if(null!=buggly_conn._readTask)
                    await buggly_conn._readTask;
                //buggly_conn._inStream.ReadTimeout = mExchangeTimeout;
                DoActionConnectSucceed();
                return res;
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