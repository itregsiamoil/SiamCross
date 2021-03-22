using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors
{
    public interface IMeasurementsDownloader
    {
        int Aviable();
        void Download(int begin, int end);
    }
}
