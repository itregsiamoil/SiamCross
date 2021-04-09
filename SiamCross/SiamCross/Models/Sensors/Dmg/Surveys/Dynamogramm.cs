using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    public class Dynamogramm: BaseSurvey
    {
        private readonly ISensor _Sensor;
        private readonly IProtocolConnection _Connection;

        public Dynamogramm(ISensor sensor)
        {
            _Sensor = sensor;
            _Connection = sensor.Connection;

        }
    }
}
