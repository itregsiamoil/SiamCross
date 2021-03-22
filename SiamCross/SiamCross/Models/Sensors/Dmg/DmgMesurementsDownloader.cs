using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DmgMesurementsDownloader: IMeasurementsDownloader
    {
        private IProtocolConnection _Connection;
        public DmgMesurementsDownloader(IProtocolConnection protConnection)
        {
            _Connection = protConnection;
        }

        public int Aviable()
        {
            return 0;
        }
        public void Download(int begin, int end)
        {

        }
    }
}
