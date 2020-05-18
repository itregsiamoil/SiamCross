using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

namespace SiamCross.Droid.Models
{
    public class SerialUsbManagerAndroid //: ISerialUsbManager
    {
        protected static string ACTION_USB_PERMISSION = "it.mobi-ware.android.USB";
        private UsbDevice _usbObj;
        //IUsbSerialPort _usbSerialPort;
        UsbManager _usbManager;
        UsbDevice _usbDevice;

        #region IUsb implementation

        //public List<string> Devices()
        //{
        //    // Список всех подключенных в данный момент устройств
        //    Context c = Android.App.Application.Context;
        //    _usbManager = (UsbManager)c.GetSystemService(Context.UsbService);

        //    #region Comment
        //    //List<string> lista = new List<string>();
        //    //try
        //    //{
        //    //    if (_usbManager.DeviceList.Count != 0)
        //    //    {
        //    //        foreach (UsbDevice device in _usbManager.DeviceList.Values)
        //    //        {
        //    //            IUsbSerialDriver driver = UsbSerialProber.DefaultProber.ProbeDevice(device);
        //    //            if (driver == null) continue;
        //    //            if (string.IsNullOrEmpty(driver.Device.DeviceName)) continue;
        //    //            _usbSerialPort = driver.Ports[0];
        //    //            var deviceConnection = _usbManager.OpenDevice(device);
        //    //            _usbSerialPort.Open(deviceConnection);
        //    //            _usbSerialPort.SetParameters(115200, 8, StopBits.One, Parity.None);
        //    //            break;
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        //lista.Add ( "-Attenzione non vedo dispositivi");
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    //lista.Add (ex.ToString ());
        //    //    throw new Exception(ex.Message);
        //    //}
        //    #endregion
        //    ConnectAndSend(new byte[] { (byte)'0', (byte)'*' }, 0, 0);
        //    return lista;
        //}

        #endregion

        public void Initialize()
        {
            _usbManager = (UsbManager)Forms.Context.GetSystemService(Context.UsbService);
            if (_usbManager.DeviceList.Count == 0)
            {
                return;
            }

            _usbDevice = _usbManager.DeviceList.First().Value;
            if (_usbDevice == null)
            {
                throw new Exception("Устройство не найдено, попробуйте настроить его в настройках");
            }

            GetPremission();
            //_usbDevice.Port
        }

        private void GetPremission()
        {
            if (!_usbManager.HasPermission(_usbDevice))
            {
                try
                {
                    PendingIntent pi = PendingIntent.GetBroadcast(Forms.Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
                    _usbManager.RequestPermission(_usbDevice, pi);
                    throw new Exception("Перезапустить пресс");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public void ConnectAndSend()
        {          
            UsbDeviceConnection deviceConnection = null;
            try
            {
                deviceConnection = _usbManager.OpenDevice(_usbDevice);                

                using (var usbInterface = _usbDevice.GetInterface(0))
                {
                    using (var usbEndpoint = usbInterface.GetEndpoint(0))
                    {
                        byte[] encodingSetting =
                            new byte[] { (byte)0x80, 0x25, 0x00, 0x00, 0x00, 0x00, 0x08 };
                        deviceConnection.ControlTransfer(
                            UsbAddressing.Out,
                            0x20,   //SET_LINE_CODING
                            0,      //value
                            0,      //index
                            encodingSetting,  //buffer
                            7,      //length
                            0);     //timeout

                        var debugOnMessage = Encoding.ASCII.GetBytes("10*5*1*");

                        deviceConnection.BulkTransfer(usbEndpoint, debugOnMessage, debugOnMessage.Length, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                // log or handle
            }
            finally
            {

                // Close the connection
                //if (deviceConnection != null)
                //{
                //    deviceConnection.Close();
                //}
            }
        }

        public void TestWrite()
        {
            throw new NotImplementedException();
        }
    }
}