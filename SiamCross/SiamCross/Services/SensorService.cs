using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models;
using SiamCross.Models.Scanners;

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

        public event Action<SensorData> SensorAdded;

        public event Action<SensorData> SensorDataChanged;

        public void SensorDataChangedHandler(SensorData data)
        {
            SensorDataChanged?.Invoke(data);
        }

        public void AddSensor(ScannedDeviceInfo deviceInfo)
        {
            foreach(var sensor in Sensors)
            {
                if(sensor.SensorData.Name == deviceInfo.Name)
                {
                    return;
                }
            }

            var addebleSensor = SensorFactory.CreateSensor(deviceInfo);
            if (addebleSensor == null)
            {
                return;
            }

            _sensors.Add(addebleSensor);
            SensorAdded?.Invoke(addebleSensor.SensorData);
        }


        public async Task DeleteSensor(int id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                await sensor.BluetoothAdapter.Disconnect();
                _sensors.Remove(sensor);
                sensor.Dispose();
            }
        }
    }
}
