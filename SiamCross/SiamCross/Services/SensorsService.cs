using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.Models;

namespace SiamCross.Services
{
    public sealed class SensorsService 
    {
        private static readonly Lazy<SensorsService> _instance =
            new Lazy<SensorsService>(() => new SensorsService());

        public SensorsService()
        {
            _sensors = new List<ISensor>();
        }

        public static SensorsService Instance { get => _instance.Value; }

        private List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public void AddSensor(ISensor sensor)
        {
            _sensors.Add(sensor);
        }
    }
}
