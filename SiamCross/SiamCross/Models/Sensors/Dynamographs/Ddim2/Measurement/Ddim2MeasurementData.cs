using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement
{
    [Preserve(AllMembers = true)]
    public class Ddim2MeasurementData
    {
        public Ddim2MeasurementReport Report { get; }
        public IReadOnlyList<byte> DynGraph { get; }
        public IReadOnlyList<byte> AccelerationGraph { get; }

        public double[,] DynGraphPoints { get; set; }

        public short ApertNumber { get; set; }
        public short ModelPump { get; set; }
        public MeasurementSecondaryParameters SecondaryParameters {get; set;}

        public string ErrorCode
        {
            get => _errorCode != null ?
                Convert.ToString(BitConverter.ToInt16(_errorCode, 0), 16) : "";
        }

        public DateTime Date => _date;

        private readonly DateTime _date;

        private readonly byte[] _errorCode;

        public Ddim2MeasurementData(Ddim2MeasurementReport report,
                           short apertNumber,
                           short modelPump,
                           List<byte> dynGraph,
                           DateTime date,
                           MeasurementSecondaryParameters secondaryParameters,
                           List<byte> accelerationGraph = null,
                           byte[] errorCode = null)
        {         
            Report = report;
            ApertNumber = apertNumber;
            ModelPump = modelPump;
            DynGraph = dynGraph;
            _date = date;
            SecondaryParameters = secondaryParameters;
            AccelerationGraph = accelerationGraph;
            _errorCode = errorCode;
        }
    }
}
