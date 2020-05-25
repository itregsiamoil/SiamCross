using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.USB
{
    public interface IUsbDataSubject
    {
        void Regisеter(IUsbDataObserver dataObserver);

        void Anregisеter(IUsbDataObserver dataObserver);

        void Notify(string address, byte[] data);
    }
}
