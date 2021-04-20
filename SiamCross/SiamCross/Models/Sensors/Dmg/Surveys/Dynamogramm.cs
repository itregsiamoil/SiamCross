using SiamCross.Models.Connection.Protocol;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    public class Dynamogramm : BaseSurvey
    {
        private readonly IProtocolConnection _Connection;

        public Dynamogramm(SensorModel sensor)
            : base(sensor, null, "Динамограмма", "запись нагрузки и перемещения")
        {
            _Connection = sensor.Connection;

        }
    }
}
