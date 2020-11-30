using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Sensors.Dmg.SiddosA3M.Measurement;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SiamCross.Models.Sensors;
using SiamCross.Models.Tools;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M
{
    public class SiddosA3MSensor : DmgBaseSensor
    {
        private SiddosA3MMeasurementManager _measurementManager;

        public SiddosA3MSensor(IProtocolConnection conn, SensorData sensorData)
            : base(conn, sensorData)
        {
        }
 
        public override async Task StartMeasurement(object measurementParameters)
        {
            SensorData.Status = "measure [0%] - started";
            IsMeasurement = true;
            SiddosA3MMeasurementStartParameters specificMeasurementParameters =
                (SiddosA3MMeasurementStartParameters)measurementParameters;
            _measurementManager = new SiddosA3MMeasurementManager(this, specificMeasurementParameters);
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