namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementStartParameters
    {
        public bool Amplification { get; }

        public bool Inlet { get; }
        public DuMeasurementSecondaryParameters SecondaryParameters { get; }
        public DuMeasurementStartParameters(bool amplification, bool inlet,
            DuMeasurementSecondaryParameters secondaryParameters)
        {
            Amplification = amplification;
            Inlet = inlet;
            SecondaryParameters = secondaryParameters;
        }
    }
}
