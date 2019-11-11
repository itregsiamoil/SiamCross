using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models
{
    public interface ISensor
    {
        IBluetoothAdapter BluetoothAdapter { get; }
        bool Alive { get; }
        void QuickReport();
        void StartMeasurement();
    }
}
