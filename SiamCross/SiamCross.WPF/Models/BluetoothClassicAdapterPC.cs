using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.WPF.Models;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothClassicAdapterPC))]
namespace SiamCross.WPF.Models
{
    public class BluetoothClassicAdapterPC : IBluetoothClassicAdapter
    {
        private DataWriter _writer;
        private DataReader _reader;
        private StreamSocket _socket;
        private RfcommDeviceService _service;

        private readonly ScannedDeviceInfo _deviceInfo;

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;

        public BluetoothClassicAdapterPC(ScannedDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
        }

        public async Task Connect()
        {
            var connectArgs = _deviceInfo.BluetoothArgs;
            if (connectArgs is DeviceInformation deviceInfo)
            {
                try
                {
                    var device = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
                    var services = await device.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(RfcommServiceId.SerialPort.Uuid), BluetoothCacheMode.Uncached);
                    //_service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);
                    Console.WriteLine("Bluetooth adress: " + device.BluetoothAddress);
                    Console.WriteLine(services.Services.Count);
                    _service = services.Services[0];
                    _socket = new StreamSocket();
                    Console.WriteLine(_service.ConnectionHostName);
                    Console.WriteLine(_service.ConnectionServiceName);
                    await _socket.ConnectAsync(
                        _service.ConnectionHostName,
                        _service.ConnectionServiceName);

                    _writer = new DataWriter(_socket.OutputStream);
                    _reader = new DataReader(_socket.InputStream);

                    ConnectSucceed?.Invoke();

                    Task taskListen = Task.Run(
                        async () => ListenForMessagesAsync(_socket));
                    taskListen.ConfigureAwait(false);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
        }

        public async Task Disconnect()
        {
            try
            {
                await _socket.CancelIOAsync();

                _socket.Dispose();
                _socket = null;

                _service.Dispose();
                _service = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async Task SendData(byte[] data)
        {
            try
            {

                Console.WriteLine($"Send DATA: {BitConverter.ToString(data)}");
                _writer.WriteBytes(data);

                await _writer.StoreAsync();

                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        private async Task ListenForMessagesAsync(StreamSocket localSocket)
        {
            while(localSocket != null)
            {
                try
                {
                    _reader.InputStreamOptions = InputStreamOptions.Partial;

                    byte[] readData = new byte[10];
                    uint s = await _reader.LoadAsync(1);
                    Console.WriteLine(s);
                    var data = _reader.ReadString(s);
                    //Console.WriteLine(BitConverter.GetBytes(data));
                    //DataReceived?.Invoke(BitConverter.GetBytes(data));
                    Console.WriteLine(data);
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
