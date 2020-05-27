using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models;
using SiamCross.Models.USB;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    public class CustomBluetooth5Adapter : IBluetoothAdapter, IUsbDataObserver
    {
        public string Address { get; private set; }
        private bool _isConnected;
        private USBService _usbService;
        private int _forceScanCounter;

        public CustomBluetooth5Adapter(string address)
        {
            Address = address;
            _isConnected = false;
            _usbService = USBService.Instance;
            _forceScanCounter = 0;

            _usbService.RegisterUsbObserver(this as IUsbDataObserver);
        }

        public async Task Connect()
        {
            try
            {
                _usbService.ConnectQuery(Address);
            }
            catch (NotSupportedException exception)
            {
                Console.WriteLine($@"Connect metod error {Address} : {exception.Message}; force scan counter = {_forceScanCounter}");
                _forceScanCounter++;
                if (_forceScanCounter % 5 == 1)
                {
                    Console.WriteLine($@"{Address} adapter forced scan!");
                    _usbService.StartScanQuery();
                    return;
                }
            }

            await Task.Delay(5000);

            if (_isConnected)
            {
                _forceScanCounter = 0;
                ConnectSucceed?.Invoke();
            }
            else
            {
                throw new TimeoutException("Connection timed out!");
            }
        }

        public async Task Disconnect()
        {
            _usbService.DisconnecDeviceQuery(Address);
        }

        public async Task SendData(byte[] data)
        {
            try
            {
                _usbService.SendDataQuery(data, Address);
            }
            catch (NotSupportedException exception)
            {
                ConnectFailed?.Invoke();
            }
        }

        public void OnDataRecieved(byte[] data)
        {
            DataReceived?.Invoke(data);
        }

        public void OnConnectSucceed()
        {
            _isConnected = true;
        }

        public void OnDisconnected()
        {
            _isConnected = false;
            ConnectFailed?.Invoke();
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                if (((CustomBluetooth5Adapter) obj).Address.Equals(Address))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;
        public event Action ConnectFailed;
    }
}