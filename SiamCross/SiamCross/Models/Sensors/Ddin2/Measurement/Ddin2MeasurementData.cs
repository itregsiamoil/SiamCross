﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Ddin2.Measurement
{
    public class Ddin2MeasurementData
    {
        public Ddin2MeasurementReport Report { get; }
        public IReadOnlyList<byte> DynGraph { get; }
        public IReadOnlyList<byte> AccelerationGraph { get; }

        public double[,] DynGraphPoints { get; set; }

        public string ErrorCode
        {
            get => _errorCode != null ?
                Convert.ToString(BitConverter.ToInt16(_errorCode, 0), 16) : "";
        }

        public string Date { get => _date.ToString(); }

        private readonly DateTime _date;

        private readonly byte[] _errorCode;

        public Ddin2MeasurementData(Ddin2MeasurementReport report,
                           List<byte> dynGraph,
                           DateTime date,
                           List<byte> accelerationGraph = null,
                           byte[] errorCode = null)
        {
            Report = report;
            DynGraph = dynGraph;
            _date = date;
            AccelerationGraph = accelerationGraph;
            _errorCode = errorCode;
        }
    }
}