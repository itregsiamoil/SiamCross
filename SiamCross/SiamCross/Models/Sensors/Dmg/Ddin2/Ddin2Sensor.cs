using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Services;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.Ddin2
{
    public class Ddin2Sensor : DmgBaseSensor
    {
        private Ddin2MeasurementManager _measurementManager;

        public Ddin2Sensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
        public override async Task StartMeasurement(object measurementParameters)
        {
            object report = null;
            try
            {
                SensorData.Status = Resource.Survey;
                IsMeasurement = true;
                Ddin2MeasurementStartParameters startParams = (Ddin2MeasurementStartParameters)measurementParameters;
                _measurementManager = new Ddin2MeasurementManager(this, startParams);
                report = await _measurementManager.RunMeasurement();
            }
            catch (Exception)
            {
                report = null;
            }
            finally
            {
                if (null != report)
                {
                    SensorService.Instance.MeasurementHandler(report);
                    SensorData.Status = Resource.Survey + ": complete";
                }
                else
                {
                    SensorData.Status = Resource.Survey + ": " + Resource.Error;
                }
                await Task.Delay(2000);
                IsMeasurement = false;
            }
        }
    }
}
