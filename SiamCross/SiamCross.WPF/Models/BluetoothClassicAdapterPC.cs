using InTheHand.Net;
using InTheHand.Net.Sockets;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.WPF.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Net.Sockets;

[assembly: Dependency(typeof(BluetoothClassicAdapterPC))]
namespace SiamCross.WPF.Models
{
    public class BluetoothClassicAdapterPC : IBluetoothClassicAdapter
    {
        private static readonly Guid _siamServiceUuid
            = new Guid("{00001101-0000-1000-8000-00805f9b34fb}");

        private readonly ScannedDeviceInfo _deviceInfo;

        private BluetoothClient _bluetoothClient;

        private Stream _stream;

        private CancellationTokenSource _cancellToken;

        private Task _readTask;

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;
        public BluetoothClassicAdapterPC(ScannedDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
        }

        public async Task Connect()
        {
            await Task.Run(() =>
            {
                if (_deviceInfo.BluetoothArgs is BluetoothDeviceInfo device)
                {
                    try
                    {
                        Guid serviceClass = _siamServiceUuid;

                        var endPoint = new BluetoothEndPoint(device.DeviceAddress, serviceClass);

                        _bluetoothClient = new BluetoothClient();
                        _bluetoothClient.Connect(endPoint);
                        
                        if(_bluetoothClient.Connected)
                        {
                            _stream = _bluetoothClient.GetStream();

                            _cancellToken = new CancellationTokenSource();
                            _readTask = new Task(() => BackgroundRead(_cancellToken));
                            _readTask.Start();
                            ConnectSucceed?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("BluetoothClassicAdapter.Connect: " + ex.Message);
                    }
                }
            });
            
        }

        public async Task Disconnect()
        {
            await Task.Run(() =>
            {
                PrivateDisconnect();
            });
        }

        private void PrivateDisconnect()
        {
            _cancellToken?.Cancel();
            _cancellToken?.Dispose();
            _cancellToken = null;
            _stream?.Close();
            _stream?.Dispose();
            _stream = null;
            _bluetoothClient?.Close();
            _bluetoothClient?.Dispose();
            _bluetoothClient = null;
        }

        public async Task SendData(byte[] data)
        {
            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
                //Console.WriteLine($"Send message: {BitConverter.ToString(data)}");
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("BluetoothClassicAdapter.SendData: " + ex.Message);
                PrivateDisconnect();
                ConnectFailed?.Invoke();
            }
        }

        private void BackgroundRead(CancellationTokenSource _cancellToken)
        {
            while (!_cancellToken.IsCancellationRequested)
            {
                if (!_stream.CanRead)
                {
                    continue;
                }
                byte[] inBuf = new byte[1];

                try
                {
                    int readLen = _stream.Read(inBuf, 0, inBuf.Length);
                    DataReceived?.Invoke(inBuf);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    PrivateDisconnect();
                    ConnectFailed?.Invoke();
                }
                
            }
        }
    }
}
