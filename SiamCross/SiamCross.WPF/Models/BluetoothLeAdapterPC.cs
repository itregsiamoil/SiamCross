using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.WPF.Models;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothLeAdapterPC))]
namespace SiamCross.WPF.Models
{
    public class BluetoothLeAdapterPC : IBluetoothLeAdapter
    {
        /// <summary>
        /// Характеристика чтения
        /// </summary>
        public GattCharacteristic WriteCaracteristic { get; private set; }

        /// <summary>
        /// Характеристика записи
        /// </summary>
        public GattCharacteristic ReadCaracteristic { get; private set; }

        /// <summary>
        /// Аргумсенты найденного устройства
        /// </summary>
        private BluetoothLEAdvertisementReceivedEventArgs _recivedDevice;

        /// <summary>
        /// Блютуз устройство
        /// </summary>
        private BluetoothLEDevice _bluetoothLeDevice;

        /// <summary>
        /// Датаврайтер
        /// </summary>
        private DataWriter byteDataWriter = new DataWriter();

        private readonly ScannedDeviceInfo _deviceInfo;

        public event Action<byte[]> DataReceived;
        public event Action ConnectSucceed;

        public BluetoothLeAdapterPC(ScannedDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
        }

        public async Task Connect()
        {
            var connectArgs = _deviceInfo.BluetoothArgs;
            if (connectArgs is BluetoothLEAdvertisementReceivedEventArgs recivedDevice)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _bluetoothLeDevice =
                        await BluetoothLEDevice.FromBluetoothAddressAsync(recivedDevice.BluetoothAddress);
                        GattDeviceServicesResult result =
                            await _bluetoothLeDevice.GetGattServicesAsync();

                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            _recivedDevice = recivedDevice;
                            Console.WriteLine("Connect with " + recivedDevice.Advertisement.LocalName
                                + Environment.NewLine + "Address: " + recivedDevice.BluetoothAddress
                                + Environment.NewLine);
                            await EnableCccdCharacteristics(result);  // CCCD Enable
                            DefineWriteReadCharacteristics(result);
                            //ConnectCompleted?.Invoke();
                            break;
                        }
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine($"Ошибка подключения. Попытка № {i}");
                    }
                }
            }
        }

        /// <summary>
        /// Включить нотификации
        /// </summary>
        /// <param name="servisesRezult"></param>
        public async Task EnableCccdCharacteristics(GattDeviceServicesResult servisesRezult)
        {
            GattCharacteristicsResult characteristicsResult;
            foreach (var service in servisesRezult.Services)
            {
                characteristicsResult = await service.GetCharacteristicsAsync();
                if (characteristicsResult.Status == GattCommunicationStatus.Success)
                {
                    var characteristics = characteristicsResult.Characteristics;
                    foreach (var characteristic in characteristics)
                    {
                        try
                        {
                            GattCommunicationStatus status = await characteristic
                                .WriteClientCharacteristicConfigurationDescriptorAsync(
                                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        }
                        catch { };
                    }
                }
            }
        }

        /// <summary>
        /// Определить характеристики чтения и записи
        /// </summary>
        /// <param name="gattDeviceServicesResult"></param>
        private async void DefineWriteReadCharacteristics(GattDeviceServicesResult gattDeviceServicesResult)
        {
            var services = gattDeviceServicesResult.Services;

            Console.WriteLine("Services: " + Environment.NewLine);
            foreach (var service in services)
            {
                Console.WriteLine("Uuid: " + service.Uuid.ToString() + Environment.NewLine);
                if (!service.Uuid.ToString()
                    .StartsWith("569a1101-b87f-490c-92cb-11ba5ea5167c"))
                {
                    continue;
                }

                GattCharacteristicsResult characteristicsResult =
                    await service.GetCharacteristicsAsync();

                if (characteristicsResult.Status == GattCommunicationStatus.Success)
                {
                    var characteristics = characteristicsResult.Characteristics;
                    Console.WriteLine("Characteristics: " + Environment.NewLine);
                    foreach (var characteristic in characteristics)
                    {
                        Console.WriteLine(characteristic.Uuid + Environment.NewLine);
                        if (characteristic.Uuid.ToString()
                            .StartsWith("569a2001-b87f-490c-92cb-11ba5ea5167")) //характеристика записи
                        {
                            WriteCaracteristic = characteristic;
                        }
                        else if (characteristic.Uuid.ToString()
                            .StartsWith("569a2000-b87f-490c-92cb-11ba5ea5167")) //характеристика чтения
                        {
                            characteristic.ValueChanged += CharacteristicValueChangedHandler;
                            ReadCaracteristic = characteristic;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик изменинияф значения характеристики
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CharacteristicValueChangedHandler(GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            DataReceived?.Invoke(input);

            Console.WriteLine("Was read: " + BitConverter.ToString(input).Replace("-", "")
                                + Environment.NewLine);
        }

        public async Task Disconnect()
        {
            if (_bluetoothLeDevice == null)
            {
                return;
            }

            GattDeviceServicesResult servisesRezult = null;
            try
            {
                _bluetoothLeDevice =
                    await BluetoothLEDevice.FromBluetoothAddressAsync(_recivedDevice.BluetoothAddress);
                servisesRezult = await _bluetoothLeDevice.GetGattServicesAsync();
                await SendStringMessageAsync("letmeout");
                await SendStringMessageAsync("drop");

                foreach (var service in servisesRezult.Services)
                {
                    service.Dispose();
                }
                _bluetoothLeDevice.Dispose();

                if (WriteCaracteristic == null && ReadCaracteristic == null)
                {
                    Console.WriteLine("Disconnect" + Environment.NewLine);
                }
                WriteCaracteristic = null;
                ReadCaracteristic = null;

                _recivedDevice = null;
            }
            catch { }
        }

        public async Task SendData(byte[] data)
        {
            if (WriteCaracteristic == null)
            {
                return;
            }

            byteDataWriter.WriteBytes(data);
            try
            {
                var result = await WriteCaracteristic.WriteValueAsync(byteDataWriter.DetachBuffer());

                switch (result)
                {
                    case GattCommunicationStatus.Success:
                        Console.WriteLine("Was send: "
                                   + BitConverter.ToString(data).Replace("-", "")
                                   + Environment.NewLine);
                        break;
                    default:
                        Console.WriteLine(result.ToString() + Environment.NewLine);
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Ошибка соединения с устройством!");
            };
        }

        /// <summary>
        /// Отправить сообщения формата string 
        /// </summary>
        /// <param name="stringMessage"></param>
        private async Task SendStringMessageAsync(string stringMessage)
        {
            if (WriteCaracteristic == null)
            {
                return;
            }

            byteDataWriter.WriteString(stringMessage);
            try
            {
                var result = await WriteCaracteristic.WriteValueAsync(byteDataWriter.DetachBuffer());

                switch (result)
                {
                    case GattCommunicationStatus.Success:
                        Console.WriteLine("Was send: " + stringMessage + Environment.NewLine);
                        break;
                    default:
                        Console.WriteLine(result.ToString() + Environment.NewLine);
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Ошибка соединения с устройством!");
            };
        }
    }
}
