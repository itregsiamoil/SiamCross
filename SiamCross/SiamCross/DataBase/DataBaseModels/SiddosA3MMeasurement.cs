﻿using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
using System;
using System.Collections.Generic;

namespace SiamCross.DataBase.DataBaseModels
{
    public class SiddosA3MMeasurement
    {
        public int Id { get; set; }

        /*/ Report /*/
        public float MaxWeight { get; set; }
        public float MinWeight { get; set; }
        public short Travel { get; set; }
        public short Period { get; set; }
        public short Step { get; set; }
        public short WeightDiscr { get; set; }
        public short TimeDiscr { get; set; }
        /*/ ----- /*/

        /*/ Graphs /*/
        public byte[] DynGraph { get; set; }
        public byte[] AccelerationGraph { get; set; }
        /*/ ----- /*/

        /*/ Secondary /*/
        public string Field { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comment { get; set; }

        public string Name { get; set; }
        /*/ ----- /*/

        /*/ Other /*/
        public DateTime DateTime { get; set; }
        public string ErrorCode { get; set; }
        public short ApertNumber { get; set; }
        public short ModelPump { get; set; }

        public short MaxBarbellWeight { get; set; }
        public short MinBarbellWeight { get; set; }
        /*/ ----- /*/

        public double TravelLength { get; set; }
        public double SwingCount { get; set; }

        public SiddosA3MMeasurement() { }
        public SiddosA3MMeasurement(SiddosA3MMeasurementData siddosA3MMeasurementData)
        {
            TravelLength = siddosA3MMeasurementData.Report.Travel 
                * siddosA3MMeasurementData.Report.Step / 10000;

            SwingCount = (60 / 0.001) / (siddosA3MMeasurementData.Report.Period 
                * siddosA3MMeasurementData.Report.TimeDiscr);

            MaxWeight = siddosA3MMeasurementData.Report.MaxWeight / 1000f;
            MinWeight = siddosA3MMeasurementData.Report.MinWeight / 1000f;
            Travel = siddosA3MMeasurementData.Report.Travel;
            Period = siddosA3MMeasurementData.Report.Period;
            Step = siddosA3MMeasurementData.Report.Step;
            WeightDiscr = siddosA3MMeasurementData.Report.WeightDiscr;
            TimeDiscr = siddosA3MMeasurementData.Report.TimeDiscr;
            ApertNumber = siddosA3MMeasurementData.ApertNumber;
            ModelPump = siddosA3MMeasurementData.ModelPump;

            if (siddosA3MMeasurementData.DynGraph != null)
            {
                DynGraph = new List<byte>(siddosA3MMeasurementData.DynGraph).ToArray();
            }
            if (siddosA3MMeasurementData.AccelerationGraph != null)
            {
                AccelerationGraph = new List<byte>(siddosA3MMeasurementData.AccelerationGraph)
                    .ToArray();
            }

            Field = siddosA3MMeasurementData.SecondaryParameters.Field;
            Well = siddosA3MMeasurementData.SecondaryParameters.Well;
            Bush = siddosA3MMeasurementData.SecondaryParameters.Bush;
            Shop = siddosA3MMeasurementData.SecondaryParameters.Shop;
            BufferPressure = siddosA3MMeasurementData.SecondaryParameters.BufferPressure;
            Comment = siddosA3MMeasurementData.SecondaryParameters.Comment;
            Name = siddosA3MMeasurementData.SecondaryParameters.DeviceName;

            MaxBarbellWeight = 0;
            MinBarbellWeight = 0;

            DateTime = siddosA3MMeasurementData.Date;
            ErrorCode = siddosA3MMeasurementData.ErrorCode;
        }
    }
}
