using System;
using System.Collections.Generic;
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
            _dynDictionary = new Dictionary<byte[], byte[]>();

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
            foreach(var pair in _dynDictionary)
            {
                if(pair.Value == null)
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
    }
}
