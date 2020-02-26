namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementStartParameters : Ddim2.Measurement.Ddim2MeasurementStartParameters
    {
        public SiddosA3MMeasurementStartParameters(float dynPeriod, int apertNumber,
            float imtravel, int modelPump, MeasurementSecondaryParameters secondaryParameters) 
            : base(dynPeriod, apertNumber, imtravel, modelPump, secondaryParameters)
        {
        }
    }
}
