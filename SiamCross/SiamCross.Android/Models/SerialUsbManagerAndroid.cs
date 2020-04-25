using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Xamarin.Forms;

namespace SiamCross.Droid.Models
{
    public class SerialUsbManagerAndroid : ISerialUsbManager
    {
        protected static string ACTION_USB_PERMISSION = "it.mobi-ware.android.USB";
        private UsbDevice _usbObj;
        IUsbSerialPort _usbSerialPort;
        UsbManager _usbManager;
        public SerialUsbManagerAndroid()
        {
            var test = new SerialUsbConnector();
        }

        #region IUsb implementation

        public List<string> Devices()
        {
            List<string> lista = new List<string>();


            // Список всех подключенных в данный момент устройств
            Context c = Android.App.Application.Context;
            _usbManager = (UsbManager)c.GetSystemService(Context.UsbService);
            try
            {
                if (_usbManager.DeviceList.Count != 0)
                {
                    foreach (UsbDevice device in _usbManager.DeviceList.Values)
                    {
                        IUsbSerialDriver driver = UsbSerialProber.DefaultProber.ProbeDevice(device);
                        if (driver == null) continue;
                        if (string.IsNullOrEmpty(driver.Device.DeviceName)) continue;
                        _usbSerialPort = driver.Ports[0];
                        var deviceConnection = _usbManager.OpenDevice(device);
                        _usbSerialPort.Open(deviceConnection);
                        _usbSerialPort.SetParameters(115200, 8, StopBits.One, Parity.None);
                        break;
                    }
                }
                else
                {
                    //lista.Add ( "-Attenzione non vedo dispositivi");
                }

            }
            catch (Exception ex)
            {
                //lista.Add (ex.ToString ());
                throw new Exception(ex.Message);
            }
            ConnectAndSend(new byte[] { (byte)'0', (byte)'*', 13, 10}, 0, 0);
            return lista;
        }

        #endregion

        public void ConnectAndSend(byte[] bytesToPrint, int productId, int vendorId)
        {
            // Получите usbManager, который может получить доступ ко всем устройствам
            //var usbManager = (UsbManager)Forms.Context.GetSystemService(Context.UsbService);

            // Получить устройство, к которому вы хотите получить доступ, из DeviceList
            // Я знаю vendorId и ProductId, но вы можете выполнить итерацию, чтобы найти тот, который вам нужен
            //var matchingDevice = _usbManager.DeviceList.FirstOrDefault(
            //    item => item.Value.VendorId == _usbObj.VendorId && item.Value.ProductId == _usbObj.ProductId);
            if (_usbManager.DeviceList.Count == 0)
                throw new Exception("Нет устройств, подключенных к USB");
            var matchingDevice = _usbManager.DeviceList.First();
            if (matchingDevice.Value == null)
                throw new Exception("Устройство не найдено, попробуйте настроить его в настройках");

            //          var usbDevice = usbManager.DeviceList.First ();
            //          if (usbManager.DeviceList.Count == 1)
            //              usbDevice = usbManager.DeviceList.First ();

            // DeviceList - это словарь с портом в качестве ключа, поэтому вытащите нужное устройство. Я тоже сохраняю порт
            var usbPort = matchingDevice.Key;
            var usbDevice = matchingDevice.Value;

            // Получаем разрешение от пользователя на доступ к устройству(иначе соединение позже будет null)
            if (!_usbManager.HasPermission(usbDevice))
            {
                try
                {
                    PendingIntent pi = PendingIntent.GetBroadcast(Forms.Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
                    _usbManager.RequestPermission(usbDevice, pi);
                    throw new Exception("Перезапустить пресс");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            // Открываем соединение с устройством
            // Я завершаю попытку, чтобы вы могли закрыть ее, если она выдает ошибку или все готово.
            UsbDeviceConnection deviceConnection = null;
            try
            {
                deviceConnection = _usbManager.OpenDevice(usbDevice);

                // Получить интерфейс usbInterface для устройства. Он и usbEndpoint реализуют IDisposable, поэтому используйте
                // Возможно, вы захотите перебрать интерфейсы, чтобы найти тот, который вам нужен (вместо 0)
                using (var usbInterface = usbDevice.GetInterface(0))
                {

                    // Получить конечную точку, снова реализуя IDisposable, и снова нужный вам индекс
                    using (var usbEndpoint = usbInterface.GetEndpoint(0))
                    {
                        byte[] encodingSetting =
                            new byte[] { (byte)0x80, 0x25, 0x00, 0x00, 0x00, 0x00, 0x08 };
                        // Сделать запрос или что вам нужно сделать
                        deviceConnection.ControlTransfer(
                            UsbAddressing.Out,
                            0x20,   //SET_LINE_CODING
                            0,      //value
                            0,      //index
                            encodingSetting,  //buffer
                            7,      //length
                            0);     //timeout
                                    //                      byte[] bytesHello = 
                                    //                          new byte[] {(byte)'H',(byte) 'e',(byte) 'l', (byte)'l', (byte)'o', (byte)' ', 
                                    //                          (byte)'f', (byte)'r', (byte)'o',(byte) 'm',(byte) ' ', 
                                    //                          (byte)'A',(byte) 'n', (byte)'d', (byte)'r',(byte) 'o', (byte)'i',(byte) 'd', (byte) 13, (byte)10};
                        deviceConnection.BulkTransfer(usbEndpoint, bytesToPrint, bytesToPrint.Length, 0);

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
                if (deviceConnection != null)
                {
                    deviceConnection.Close();
                }
            }       
    }
}
}