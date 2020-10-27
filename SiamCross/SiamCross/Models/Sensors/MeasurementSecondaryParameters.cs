using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors
{
    public class MeasurementSecondaryParameters
    {
        public MeasurementSecondaryParameters(
            string name,
            string measurementType,
            string field,
            string well,
            string bush,
            string shop,
            string bufferPressure,
            string comment,
            string battery = "0.0",
            string temperature = "0.0",
            string mainfirmware="0.0.0",
            string radiofirmware = "0.0.0"
            )
        {
            DeviceName = name;
            MeasurementType = measurementType;
            Field = field;
            Well = well;
            Bush = bush;
            Shop = shop;
            BufferPressure = bufferPressure;
            Comment = comment;
            BatteryVolt = battery;
            Temperature = temperature;
            MainFirmware = mainfirmware;
            RadioFirmware = radiofirmware;
        }

        public string DeviceName { get; }
        public string MeasurementType { get; }
        public string Field { get; }
        public string Well { get; }
        public string Bush { get; }
        public string Shop { get; }
        public string BufferPressure { get; }
        public string Comment { get; }
        public string BatteryVolt { get; set; }
        public string Temperature { get; set; }
        public string MainFirmware { get; set; }
        public string RadioFirmware { get; set; }
        

    }
}
