using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SiamCross.WPF.Models
{
    public class BluetoothAdapterClassicPC : IBluetoothAdapter
    {
        private DataWriter _writer;
        private DataReader _reader;
        private StreamSocket _socket;
        private RfcommDeviceService _service;

        public event Action<byte[]> DataReceived;

        public async Task Connect(object connectArgs)
        {
            if (connectArgs is DeviceInformation deviceInfo)
            {
                try
                {
                    _service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);

                    _socket = new StreamSocket();

                    await _socket.ConnectAsync(
                        _service.ConnectionHostName,
                        _service.ConnectionServiceName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
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
            }
        }

        public async Task SendData(byte[] data)
        {
            try
            {
                _writer = new DataWriter(_socket.OutputStream);
                _reader = new DataReader(_socket.InputStream);

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
            }
        }
    }
}
