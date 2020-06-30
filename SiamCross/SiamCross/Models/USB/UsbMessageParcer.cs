using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiamCross.Models.Scanners;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.USB
{
    public class UsbMessageParcer
    {
        public void Parse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.Length > 0)
            {
                if (message[0] != '$')
                {
                    return;
                }
            }

            var messages = GetMessageBlocks(message, '$');

            foreach (var singleMessage in messages)
            {
                var messageType = DefineMessageType(singleMessage);

                switch (messageType)
                {
                    case UsbMessageType.Scan:
                        ParceScanMessage(singleMessage);
                        break;
                    case UsbMessageType.StopScan:
                        ParceStopScanMessage(singleMessage);
                        break;
                    case UsbMessageType.Connection:
                        ParceConnectionMessage(singleMessage);
                        break;
                    case UsbMessageType.Data:
                        ParceDataMessage(singleMessage);
                        break;
                    case UsbMessageType.Disconnect:
                        ParceDisconnectMessage(singleMessage);
                        break;
                    case UsbMessageType.NoSupport:
                    default:
                        break;
                }
            }
        }

        private UsbMessageType DefineMessageType(string message)
        {
            if (message.Length < 2)
            {
                return UsbMessageType.NoSupport;
            }
            switch (message[1])
            {
                case '1':
                    return UsbMessageType.Scan;
                case '2':
                    return UsbMessageType.StopScan;
                case '3':
                    return UsbMessageType.Connection;
                case '7':
                    return UsbMessageType.Data;
                case '9':
                    return UsbMessageType.Disconnect;
                default:
                    return UsbMessageType.NoSupport;
            }
        }


        private void ParceScanMessage(string message)
        {
            if (message.Contains("Scan started"))
            {
                ScanStarted?.Invoke();
            }
            else
            {
                var messageBlocks = GetMessageBlocks(message, '*');
                var address = messageBlocks[2];
                var name = messageBlocks[3];

                var scannedDeviceInfo = new ScannedDeviceInfo(
                    name, address, BluetoothType.UsbCustom5);

                DeviceFounded?.Invoke(scannedDeviceInfo);
            }
        }

        private void ParceStopScanMessage(string message)
        {
            if (!message.Contains("Scan stopped") &&
                !message.Contains("Scanning stopped") &&
                !message.Contains("dev in table") &&
                !message.Contains("device in table"))
            {
                var messageBlocks = GetMessageBlocks(message, '*');
                var numberInTable = int.Parse(messageBlocks[1]);
                var address = messageBlocks[2];
                var name = messageBlocks[3];

                TableComponentRecieved?.Invoke(numberInTable, address, name);
            }
            else if(message.Contains("Scan stopped"))
            {
                ScanStopped?.Invoke();
            }
        }

        private void ParceConnectionMessage(string message)
        {
            string stringNumberInTable = null;
            if (message.Contains("for dev"))
            {
                stringNumberInTable = 
                    
                    GetMessageBlocks(
                        GetMessageBlocks(
                            GetMessageBlocks(message, '*')
                                [1], '№')
                            [1], ' ')[0];
            }
            if(message.Contains("Connection already exist"))
            {
                stringNumberInTable = GetMessageBlocks(message, ' ').Last();
            }

            if(stringNumberInTable != null)
            {
                int numberInTable;

                if (int.TryParse(stringNumberInTable, out numberInTable))
                {
                    DeviceConnected?.Invoke(numberInTable);
                }
            }
        }

        private void ParceDisconnectMessage(string message)
        {
            if (message.Contains("Disconnected"))
            {
                var numberInTableStr = GetMessageBlocks(message, '*')[1];

                int numberInTable;

                if (int.TryParse(numberInTableStr, out numberInTable))
                {
                    DeviceDisconnected?.Invoke(numberInTable);
                }
            }
        }

        private void ParceDataMessage(string message)
        {
            try
            {
                var payloadData = GetMessageBlocks(message, '*')[3];

                var numberInTableStr = GetMessageBlocks(message, '*')[1];
                int numberInTable;

                payloadData = payloadData.Replace(System.Environment.NewLine, string.Empty);

                if (int.TryParse(numberInTableStr, out numberInTable))
                {
                    DataRecieved?.Invoke(numberInTable, StringToByteArray(payloadData));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UsbDataRecieve parce error\n");
                Console.WriteLine($"Message = {message}\n");
            }
            
        }

        private List<string> GetMessageBlocks(string message, char separator)
        {

            return message.Split(separator)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
        }

        public byte[] StringToByteArray(string hex)
        {
            List<byte> bytes = new List<byte>();
            try
            {
                bytes = Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return bytes.ToArray();
        }

        public event Action ScanStarted;
        public event Action ScanStopped;
        public event Action<ScannedDeviceInfo> DeviceFounded;
        public event Action<int, string, string> TableComponentRecieved;
        public event Action<int, byte[]> DataRecieved;
        public event Action<int> DeviceConnected;
        public event Action<int> DeviceDisconnected;
    }

    public enum UsbMessageType
    {
        Scan = 1,
        StopScan = 2,
        Connection = 3,
        Data = 7,
        Disconnect = 9,
        NoSupport = -1
    }
}
