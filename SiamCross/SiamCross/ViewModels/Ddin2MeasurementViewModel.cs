using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.ViewModels
{
    public class Ddin2MeasurementViewModel : BaseViewModel, IViewModel
    {
        private SensorData _sensorData;

        public Ddin2MeasurementViewModel(SensorData sensorData)
        {
            _sensorData = sensorData;
            SensorName = _sensorData.Name;
        }

        public string SensorName 
        { 
            get; 
            set; 
        }
    }
}
