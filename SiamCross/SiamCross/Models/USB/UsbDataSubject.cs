using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SiamCross.Models.USB
{
    public class UsbDataDataSubject : IUsbDataSubject
    {
        private List<IUsbDataObserver> _observerList;

        public UsbDataDataSubject()
        {
            _observerList = new List<IUsbDataObserver>();
        }
        public void Regisеter(IUsbDataObserver dataObserver)
        {
            _observerList.Add(dataObserver);
        }

        public void Anregisеter(IUsbDataObserver dataObserver)
        {
            if(_observerList.Contains(dataObserver))
            {
                _observerList.Remove(dataObserver);
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
