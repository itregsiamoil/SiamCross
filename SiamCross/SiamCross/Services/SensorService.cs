using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.Models;

namespace SiamCross.Services
{
    public sealed class SensorService 
    {
        private static readonly Lazy<SensorService> _instance =
            new Lazy<SensorService>(() => new SensorService());

        public int SensorsCount => _sensors.Count;

        private SensorService()
        {
            _sensors = new List<ISensor>();
        }

        public static SensorService Instance { get => _instance.Value; }

        private List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public void AddSensor(ISensor sensor)
        {
            foreach(var currentSensor in Sensors)
            {
                if(currentSensor.SensorData.Name == sensor.SensorData.Name)
                {
                    return;
                }
            }

            _sensors.Add(sensor);
        }
    }
}
