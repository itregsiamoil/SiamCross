using SiamCross.Models.Connection.Protocol;

namespace SiamCross.Models.Sensors.Dmg.Surveys
{
    public class Dynamogramm : BaseSurveyModel
    {
        private readonly IProtocolConnection _Connection;

        public Dynamogramm(SensorModel sensor)
            : base(sensor, null, Resource.Dynamogram, Resource.RecordingLoadAndMovement)
        {
            _Connection = sensor.Connection;

        }
    }
}
