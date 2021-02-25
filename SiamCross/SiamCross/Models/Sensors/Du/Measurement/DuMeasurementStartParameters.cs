namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementStartParameters
    {
        public bool Depth6000 { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; }
        public double PumpDepth { get; set; }//Глубина подвески насоса
        public DuMeasurementSecondaryParameters SecondaryParameters { get; }
        public DuMeasurementStartParameters(bool amplification, bool inlet, bool depth6000,
            DuMeasurementSecondaryParameters secondaryParameters, double pump_depth)
        {
            Amplification = amplification;
            Inlet = inlet;
            Depth6000 = depth6000;
            PumpDepth = pump_depth;
            SecondaryParameters = secondaryParameters;
        }
    }
}
