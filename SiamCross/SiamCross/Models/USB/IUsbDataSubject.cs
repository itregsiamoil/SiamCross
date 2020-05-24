using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.USB
{
    public interface IUsbDataSubject
    {
        void Regisеter(IUsbObserver observer);

        void Anregisеter(IUsbObserver observer);

        void Notify(string address, byte[] data);
    }
}
