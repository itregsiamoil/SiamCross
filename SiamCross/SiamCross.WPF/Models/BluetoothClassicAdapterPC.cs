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
                        _stream = _bluetoothClient.GetStream();
                        ConnectSucceed?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("BluetoothClassicAdapter.SendData: " + ex.Message);
                    }
                }
            });
            
        }

        public async Task Disconnect()
        {
            await Task.Run(() =>
            {
                _stream?.Close();
                _stream?.Dispose();
                _stream = null;
                _bluetoothClient?.Close();
                _bluetoothClient?.Dispose();
                _bluetoothClient = null;
            });
        }

        public async Task SendData(byte[] data)
        {
            try
            {
                short dataLength = BitConverter.ToInt16(new byte[] { data[8], data[9] }, 0);
                if (data[3] == 1)//Команда чтения
                {
                    await _stream?.WriteAsync(data, 0, data.Length);

                    await Task.Delay(300);

                    int messageLength = 14 + dataLength;

                    byte[] readBuffer = new byte[messageLength];

                    int readLength = await _stream?.ReadAsync(readBuffer, 0, readBuffer.Length);

                    if (readLength > 0)
                    {
                        if (readBuffer[3] == 81)
                        {
                            DataReceived?.Invoke(readBuffer.Take(14).ToArray());
                        }
                        else
                        {
                            DataReceived?.Invoke(readBuffer);
                        }
                    }
                }
                else if (data[3] == 2)//Команда записи
                {
                    await _stream?.WriteAsync(data, 0, data.Length);

                    await Task.Delay(300);

                    int messageLength = 14;

                    byte[] readBuffer = new byte[messageLength];

                    int readLength = await _stream?.ReadAsync(readBuffer, 0, readBuffer.Length);

                    if (readLength > 0)
                    {
                        if (readBuffer[3] == 2)
                        {
                            DataReceived?.Invoke(readBuffer.Take(12).ToArray());
                        }
                        else
                        {
                            DataReceived?.Invoke(readBuffer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BluetoothClassicAdapter.SendData: " + ex.Message);
                ConnectFailed?.Invoke();
            }
        }
    }
}
