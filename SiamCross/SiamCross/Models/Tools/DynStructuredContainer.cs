using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class DynStructuredContainer
    {
        private Dictionary<byte[], byte[]> _dynDictionary;

        public DynStructuredContainer()
        {
            CreateAddressesDictionary();
        }

        private void CreateAddressesDictionary()
        {
            _dynDictionary = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

            var startAddress = new byte[] { 0x00, 0x00, 0x00, 0x81 };
            var currentAddress = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            _dynDictionary.Add(startAddress, null);

            for (int i = 0; i < 198; i++)
            {
                short newAdress = (short)(BitConverter.ToInt16(
                    new byte[] { currentAddress[0], currentAddress[1] }, 0) + 20);
                currentAddress = BitConverter.GetBytes(newAdress);

                _dynDictionary.Add(new byte[]
                {
                    currentAddress[0],
                    currentAddress[1],
                    startAddress[2],
                    startAddress[3]
                }, null);
            }
        }

        public List<byte[]> GetEmptyAddresses()
        {
            var emptyAddresses = new List<byte[]>();
            foreach (var pair in _dynDictionary.ToList())
            {
                if (pair.Value == null)
                {
                    emptyAddresses.Add(pair.Key);
                }
            }
            return emptyAddresses;
        }

        public List<byte[]> GetDynData()
        {
            var dynData = new List<byte[]>();
            foreach (var pair in _dynDictionary)
            {
                if (pair.Value == null)
                {
                    return null;
                }
                else
                {
                    dynData.Add(pair.Value);
                }
            }
            return dynData;
        }

        public void AddData(byte[] address, byte[] data)
        {
            _dynDictionary[address] = data;
            Debug.WriteLine($"{BitConverter.ToString(address)} : {BitConverter.ToString(data)}\n");
            Debug.WriteLine($"Dict count = {_dynDictionary.Count}\n");
        }
    }

    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            int sum = 0;
            foreach (byte cur in key)
            {
                sum += cur;
            }
            return sum;
        }
    }
}
