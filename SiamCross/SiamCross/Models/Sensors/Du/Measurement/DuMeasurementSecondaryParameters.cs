namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementSecondaryParameters : MeasurementSecondaryParameters
    {
        public string ResearchType { get; set; }
        public string SoundSpeedCorrection { get; set; }
        public string SoundSpeed { get; set; }

        public DuMeasurementSecondaryParameters()
            :base()
        {
            //ResearchType = "";
            //SoundSpeedCorrection = "";
            //SoundSpeed = "";
        }

        public DuMeasurementSecondaryParameters(
            string name, string measurementType, 
            string field, string well, 
            string bush, string shop, 
            string bufferPressure, string comment,
            string battery,
            string temperature,
            string mainfirmware,
            string radiofirmware,
            string researchType, string soundSpeedCorrection,
            string soundSpeed
            ) : base(name, measurementType, field, well, bush, shop, bufferPressure, comment
                , battery, temperature, mainfirmware, radiofirmware)
        {
            ResearchType = researchType;
            SoundSpeedCorrection = soundSpeedCorrection;
            SoundSpeed = soundSpeed;
        }
    }
}
