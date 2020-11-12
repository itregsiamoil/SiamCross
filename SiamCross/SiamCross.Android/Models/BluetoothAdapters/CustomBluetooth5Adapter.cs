using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.USB;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    public class CustomBluetooth5Adapter : IBluetooth5CustomAdapter, IUsbDataObserver
    {
        readonly IPhyInterface mInterface;
        public IPhyInterface PhyInterface
        {
            get => mInterface;
        }

        public string Address { get; private set; }
        private bool _isConnected;
        private USBService _usbService;
        private static int _forceScanCounter;

        public CustomBluetooth5Adapter(ScannedDeviceInfo deviceInfo)
        {
            Address = deviceInfo.BluetoothArgs as string;
            _isConnected = false;
            _usbService = USBService.Instance;
            _forceScanCounter = 0;

            _usbService.RegisterUsbObserver(this as IUsbDataObserver);
        }
        public Task<byte[]> Exchange(byte[] req)
        {
            return null;
        }
        public async Task<bool> Connect()
        {
            try
            {
                await _usbService.ConnectQuery(Address);
            }
            catch (NotSupportedException exception)
            {
                Console.WriteLine($@"Connect metod error {Address} : {exception.Message}; force scan counter = {_forceScanCounter}");
                _forceScanCounter++;
                if (_forceScanCounter % 15 == 1)
                {
                    Console.WriteLine($@"{Address} adapter forced scan!");
                    await _usbService.StartScanQuery();
                    return false;
                }
            }

            await Task.Delay(5000);
            return true;

            //if (_isConnected)
            //{
            //    _forceScanCounter = 0;
            //    ConnectSucceed?.Invoke();
            //}
            //else
            //{
            //    //throw new TimeoutException("Connection timed out!");
            //}
        }

        public async Task Disconnect()
        {
            await _usbService.DisconnecDeviceQuery(Address);
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
            _forceScanCounter = 0;
            ConnectSucceed?.Invoke();
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