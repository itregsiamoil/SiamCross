using System;
using System.Collections.Generic;

namespace SiamCross.Models.Sensors.Dmg.Ddin2.Measurement
{
    public class Ddin2MeasurementData
    {
        public DmgBaseMeasureReport Report { get; }
        public IReadOnlyList<byte> DynGraph { get; }
        public IReadOnlyList<byte> AccelerationGraph { get; }

        public double[,] DynGraphPoints { get; set; }

        public short ApertNumber { get; set; }
        public short ModelPump { get; set; }

        public short Rod { get; set; }

        public MeasurementSecondaryParameters SecondaryParameters { get; set; }

        public string ErrorCode => _errorCode != null ?
                Convert.ToString(BitConverter.ToInt16(_errorCode, 0), 16) : "";

        public DateTime Date => _date;

        private readonly DateTime _date;
        private readonly byte[] _errorCode;

        public Ddin2MeasurementData(DmgBaseMeasureReport report,
                           short apertNumber,
                           short modelPump,
                           short rod,
                           List<byte> dynGraph,
                           DateTime date,
                           MeasurementSecondaryParameters secondaryParameters,
                           List<byte> accelerationGraph = null,
                           byte[] errorCode = null)
        {
            Report = report;
            ApertNumber = apertNumber;
            ModelPump = modelPump;
            Rod = rod;
            DynGraph = dynGraph;
            _date = date;
            SecondaryParameters = secondaryParameters;
            AccelerationGraph = accelerationGraph;
            _errorCode = errorCode;
        }
    }
}
