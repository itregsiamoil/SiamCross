using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.WPF.Models;
using System;
using System.Threading.Tasks;
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
                    _service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);

                    _socket = new StreamSocket();

                    await _socket.ConnectAsync(
                        _service.ConnectionHostName,
                        _service.ConnectionServiceName);
                    ConnectSucceed?.Invoke();
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
                _writer = new DataWriter(_socket.OutputStream);
                _reader = new DataReader(_socket.InputStream);

                Console.WriteLine($"Send DATA: {BitConverter.ToString(data)}");
                _writer.WriteBytes(data);

                await _writer.StoreAsync();

                await Task.Delay(300);

                byte[] readData = new byte[_reader.UnconsumedBufferLength];
                _reader.ReadBytes(readData);
                Console.WriteLine($"Read DATA: {BitConverter.ToString(readData)}");
                DataReceived?.Invoke(readData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
