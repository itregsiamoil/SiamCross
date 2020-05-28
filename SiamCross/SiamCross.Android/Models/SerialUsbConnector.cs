using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using Xamarin.Forms;

using Hoho.Android.UsbSerial.Extensions;

namespace SiamCross.Droid.Models
{
    public class SerialUsbConnector : ISerialUsbManager
    {
        private UsbSerialPort _port;
        private UsbManager _usbManager;
        private SerialInputOutputManager _serialIoManager;

        public event Action<string> DataReceived;
        public event Action ErrorReceived;

        public async Task<bool> Initialize()
        {
            _usbManager = Android.App.Application.Context.GetSystemService(Context.UsbService) as UsbManager;
            var drivers = await FindAllDriversAsync(_usbManager);

            if (drivers.Count == 0) return false;

            var driver = drivers.ToArray()[0];
            if (driver == null)
                throw new Exception("Driver specified in extra tag not found.");
            _port = driver.Ports[0];           

            if (_port == null)
            {
                return false;
            }

            await GetPremission(driver.Device);

            var portInfo = new UsbSerialPortInfo(_port);

            int vendorId = portInfo.VendorId;
            int deviceId = portInfo.DeviceId;
            int portNumber = portInfo.PortNumber;

             _port = driver.Ports[portNumber];           

            if (_port == null)
            {
                return false;
            }

            _serialIoManager = new SerialInputOutputManager(_port)
            {
                BaudRate = 115200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };


            _serialIoManager.DataReceived += (sender, e) =>
            {
                var str = Encoding.Default.GetString(e.Data);
                System.Diagnostics.Debug.WriteLine("\nModem: " + str + "\n");
                DataReceived?.Invoke(str);
            };

            _serialIoManager.ErrorReceived += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("\nModem error: " + e.ToString() + "\n");

                ErrorReceived?.Invoke();
            };

            try
            {
                _serialIoManager.Open(_usbManager);
            }
            catch (Java.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public void Write(string message)
        {
            WriteData(message + "\r\n");
        }

        private Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG

            table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

            table.AddProduct(0x10C4, 0x8293, typeof(Cp21xxSerialDriver));

            var prober = new UsbSerialProber(table);
            return prober.FindAllDriversAsync(usbManager);
        }
        
        private async Task<bool> GetPremission(UsbDevice device)
        {
            var premissionCompletion = new TaskCompletionSource<bool>();
            bool premissionGranted;
            if (!_usbManager.HasPermission(device))
            {
                try
                {
                    premissionGranted = await _usbManager.RequestPermissionAsync(device, Forms.Context);
                }
                catch (Exception ex)
                {
                    premissionGranted = false;
                }
            }
            else
            {
                premissionGranted = true;
            }

            premissionCompletion.SetResult(premissionGranted);
            return await premissionCompletion.Task;
        }

        private void WriteData(string str)
        {
            try
            {
                if (_serialIoManager.IsOpen)
                {
                    List<byte> buff = new List<byte>();
                    buff.AddRange(Encoding.ASCII.GetBytes(str));
                    _port.Write(buff.ToArray(), 200);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Write data error!");
            }
        }

        public void Disconnect()
        {
            if (_serialIoManager != null)
            {
                if (_serialIoManager.IsOpen)
                {
                    try
                    {
                        _serialIoManager.Close();
                    }
                    catch (InvalidOperationException e)
                    { }
                }
                
                _serialIoManager.Dispose();
                _serialIoManager = null;
            }
        }
    }
}