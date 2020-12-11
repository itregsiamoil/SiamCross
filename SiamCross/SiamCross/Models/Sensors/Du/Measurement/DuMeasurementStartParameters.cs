namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementStartParameters
    {
        public bool Depth6000 { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; }
        public DuMeasurementSecondaryParameters SecondaryParameters { get; }
        public DuMeasurementStartParameters(bool amplification, bool inlet,bool depth6000,
            DuMeasurementSecondaryParameters secondaryParameters)
        {
            Amplification = amplification;
            Inlet = inlet;
            Depth6000 = depth6000;
            SecondaryParameters = secondaryParameters;
        }
    }
}
