using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Text;
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
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            Ddin2MeasurementStartParameters specificMeasurementParameters =
                (Ddin2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddin2MeasurementManager(this, specificMeasurementParameters);
            var report = await _measurementManager.RunMeasurement();
            if (null != report)
            {
                SensorService.Instance.MeasurementHandler(report);
                SensorData.Status = "measure [100%] - end";
            }
            else
            {
                SensorData.Status = "measure [---%] - ERROR";
            }
            await Task.Delay(2000);
            IsMeasurement = false;
        }

    }
}
