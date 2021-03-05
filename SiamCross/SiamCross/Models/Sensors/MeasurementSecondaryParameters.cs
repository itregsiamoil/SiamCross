namespace SiamCross.Models.Sensors
{
    public enum MeasureState
    {
        Ok = 0
        , IOError = 1
        , LogicError = 2
        , UnknownError = 10
    }
    public class MeasurementSecondaryParameters
    {
        public MeasurementSecondaryParameters(
            string name = "",
            string measurementType = "",
            string field = "",
            string well = "",
            string bush = "",
            string shop = "",
            double bufferPressure = 0.0,
            string comment = "",
            string battery = "0.0",
            string temperature = "0.0",
            string mainfirmware = "0.0.0",
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

        public string DeviceName { get; set; }
        public string MeasurementType { get; set; }
        public string Field { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public double BufferPressure { get; set; }
        public string Comment { get; set; }
        public string BatteryVolt { get; set; }
        public string Temperature { get; set; }
        public string MainFirmware { get; set; }
        public string RadioFirmware { get; set; }


    }
}
