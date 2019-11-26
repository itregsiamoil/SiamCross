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

namespace SiamCross.Droid.Models
{
    public class BluetoothClassicAdapterMobile : IBluetoothClassicAdapter
    {
        private BluetoothDevice _bluetoothDevice;
        private BluetoothSocket _socket;
        private BufferedReader _reader;
        private InputStreamReader _inputStreamReader;
        private ScannedDeviceInfo _scannedDeviceInfo;

        private Stream _outStream;
        private Stream _inStream;

        private Task _readTask;

        private const string _uuid = "00001101-0000-1000-8000-00805f9b34fb";

        public BluetoothClassicAdapterMobile(ScannedDeviceInfo deviceInfo)
        {
            _scannedDeviceInfo = deviceInfo;
            _reader = null;
        }

        public async Task Connect()
        {
            Disconnect();
            _bluetoothDevice = (BluetoothDevice)_scannedDeviceInfo.BluetoothArgs;
            try
            {
                _socket = _bluetoothDevice.CreateRfcommSocketToServiceRecord(_bluetoothDevice.GetUuids()[0].Uuid);

                _socket.Connect();

                _outStream = _socket.OutputStream;
                _inStream = _socket.InputStream;

                _inputStreamReader = new InputStreamReader(_inStream);
                _reader = new BufferedReader(_inputStreamReader);

                await Task.Delay(2000);

                _cancellToken = new CancellationTokenSource();
                _readTask = new Task(() => BackgroundRead(_cancellToken));
                _readTask.Start();

                ConnectSucceed?.Invoke();
            }
            catch(Java.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                Disconnect();
            }
        }

        public async Task Disconnect()
        {
            _cancellToken.Cancel();
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
                connectedObject.Dispose();
            }
            catch (Exception)
            {
                Disconnect();
                throw;
            }

            connectedObject = null;
        }

        private static CancellationTokenSource _cancellToken;

        private void BackgroundRead(CancellationTokenSource _cancellToken)
        {
            while (!_cancellToken.IsCancellationRequested)
            {
                if (!_inStream.CanRead || !_inStream.IsDataAvailable())
                {
                    continue;
                }
                byte[] inBuf = new byte[1];

                int readLen = _inStream.Read(inBuf, 0, inBuf.Length);
                DataReceived?.Invoke(inBuf);
            }
        }

        public async Task SendData(byte[] data)
        {
             await _outStream.WriteAsync(data);
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
    }
}