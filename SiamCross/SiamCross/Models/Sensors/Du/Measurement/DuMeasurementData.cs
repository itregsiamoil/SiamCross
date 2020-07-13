using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementData
    {
        public DuMeasurementSecondaryParameters SecondaryParameters {get;set;}
        public List<byte> Echogram { get; }
        public int FluidLevel { get; set; }
        public float AnnularPressure { get; }
        public int NumberOfReflections { get; }
        public DateTime Date { get; }

        public DuMeasurementData(List<byte> echogram, 
                                 int fluidLevel,
                                 float annualPressure,
                                 int numberOfReflections,
                                 DateTime date,
                                 DuMeasurementSecondaryParameters secondaryParameters)
        {
            Echogram = echogram;
            FluidLevel = fluidLevel;
            AnnularPressure = annualPressure;
            NumberOfReflections = numberOfReflections;
            Date = date;
            SecondaryParameters = secondaryParameters;
        }
    }
}
