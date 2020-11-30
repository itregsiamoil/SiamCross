using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg.Ddim2.Measurement;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Models.Sensors.Dmg.SiddosA3M;

namespace SiamCross.Models.Sensors.Dmg.Ddim2
{
    public class Ddim2Sensor : SiddosA3MSensor
    {
        public Ddim2Sensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
    }
}
