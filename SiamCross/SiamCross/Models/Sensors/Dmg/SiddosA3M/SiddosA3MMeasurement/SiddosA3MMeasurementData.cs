﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M.Measurement
{
    public class SiddosA3MMeasurementData
    {
        public DmgBaseMeasureReport Report { get; }
        public IReadOnlyList<byte> DynGraph { get; }
        public IReadOnlyList<byte> AccelerationGraph { get; }

        public double[,] DynGraphPoints { get; set; }

        public short ApertNumber { get; set; }
        public short ModelPump { get; set; }
        public MeasurementSecondaryParameters SecondaryParameters { get; set; }

        public string ErrorCode
        {
            get => _errorCode != null ?
                Convert.ToString(BitConverter.ToInt16(_errorCode, 0), 16) : "";
        }

        public DateTime Date => _date;

        private readonly DateTime _date;

        private readonly byte[] _errorCode;

        public SiddosA3MMeasurementData(DmgBaseMeasureReport report,
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