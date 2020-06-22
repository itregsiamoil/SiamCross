using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.USB
{
    public class HardwareDevicesTable
    {
        private ConcurrentBag<HardwareDeviceTableItem> _itemBag;

        public HardwareDevicesTable()
        {
            _itemBag = new ConcurrentBag<HardwareDeviceTableItem>();
        }

        public void Clear()
        {
            HardwareDeviceTableItem someItem;
            while (!_itemBag.IsEmpty)
            {
                _itemBag.TryTake(out someItem);
            }
        }

        public void AddOrRefresh(int number, string address)
        {
            foreach (var item in _itemBag)
            {
                if (item.Number == number)
                {
                    item.Address = address;
                    return;
                }
            }

            _itemBag.Add(new HardwareDeviceTableItem(number, address));
        }

        public string GetAddressForNumber(int number)
        {
            foreach (var item in _itemBag)
            {
                if (item.Number == number)
                {
                    return item.Address;
                }
            }

            return null;
        }

        public int GetNumberForAddress(string address)
        {
            foreach (var item in _itemBag)
            {
                if (item.Address == address)
                {
                    return item.Number;
                }
            }

            return -1;
        }

        public List<string> GetAllAddresses()
        {
            var addresses = new List<string>();
            foreach (var item in _itemBag)
            {
                addresses.Add(item.Address);
            }

            return addresses;
        }
    }

    internal class HardwareDeviceTableItem
    {
        public int Number { get; set; }
        public string Address { get; set; }

        public HardwareDeviceTableItem(int number, string address)
        {
            Number = number;
            Address = address;
        }
    }
}
