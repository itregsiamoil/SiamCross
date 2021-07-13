using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Services;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.Ddin2
{
    public class Ddin2Sensor : DmgBaseSensor
    {
        private Ddin2MeasurementManager _measurementManager;

        public Ddin2Sensor(SensorModel model)
            : base(model)
        {
        }
        public override async Task StartMeasurement(object measurementParameters)
        {
            object report = null;
            try
            {
                if (IsMeasurement)
                    return;
                Status = Resource.Survey;
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
                    await SensorService.MeasurementHandler(report);
                    Status = Resource.Survey + ": " + Resource.Stat_Complete;
                }
                else
                {
                    Status = Resource.Survey + ": " + Resource.Error;
                }
                await Task.Delay(2000);
                IsMeasurement = false;
            }
        }
    }
}
