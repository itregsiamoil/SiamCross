using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SiamCross.Models.USB
{
    public class UsbDataDataSubject : IUsbDataSubject
    {
        private List<IUsbObserver> _observerList;

        public UsbDataDataSubject()
        {
            _observerList = new List<IUsbObserver>();
        }
        public void Regisеter(IUsbObserver observer)
        {
            _observerList.Add(observer);
        }

        public void Anregisеter(IUsbObserver observer)
        {
            if(_observerList.Contains(observer))
            {
                _observerList.Remove(observer);
            }
        }

        public void Notify(string address, byte[] data)
        {
            foreach (var observer in _observerList)
            {
                if (observer.Address.Equals(address))
                {
                    observer.Update(data);
                }
            }
        }
    }
}
