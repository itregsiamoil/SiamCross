using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementData
    {
        public DuMeasurementSecondaryParameters SecondaryParameters {get;set;}
        public List<byte> Echogram { get; }
        public int FluidLevel { get; }
        public int AnnularPressure { get; }
        public float SoundSpeed { get; }
        public int NumberOfReflections { get; }
        public DateTime Date { get; }

        public DuMeasurementData(List<byte> echogram, 
                                 int fluidLevel,
                                 int annualPressure,
                                 float soundSpeed,
                                 int numberOfReflections,
                                 DateTime date,
                                 DuMeasurementSecondaryParameters secondaryParameters)
        {
            Echogram = echogram;
            FluidLevel = fluidLevel;
            AnnularPressure = annualPressure;
            SoundSpeed = soundSpeed;
            NumberOfReflections = numberOfReflections;
            Date = date;
            SecondaryParameters = secondaryParameters;
        }
    }
}
