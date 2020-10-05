using System;
using System.Collections.Generic;
using SiamCross.Models.Sensors.Ddin2.Measurement;


namespace SiamCross.DataBase.DataBaseModels
{
    public class Ddin2Measurement
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

        public double Rod { get; set; }

        public short MaxBarbellWeight { get; set; }
        public short MinBarbellWeight { get; set; }
        /*/ ----- /*/

        public double TravelLength { get; set; }
        public double SwingCount { get; set; }

        public Ddin2Measurement() { }
        public Ddin2Measurement(Ddin2MeasurementData ddin2MeasurementData)
        {
            TravelLength = (double)ddin2MeasurementData.Report.Travel *
                ddin2MeasurementData.Report.Step / 10000;

            SwingCount = Math.Round((60 / 0.001) / (ddin2MeasurementData.Report.Period *
                ddin2MeasurementData.Report.TimeDiscr), 5);

            MaxWeight = ddin2MeasurementData.Report.WeightDiscr *
                ddin2MeasurementData.Report.MaxWeight / 1000f;
            MinWeight = ddin2MeasurementData.Report.WeightDiscr *
                ddin2MeasurementData.Report.MinWeight / 1000f;

            Travel = ddin2MeasurementData.Report.Travel;
            Period = ddin2MeasurementData.Report.Period;
            Step = ddin2MeasurementData.Report.Step;
            WeightDiscr = ddin2MeasurementData.Report.WeightDiscr;
            TimeDiscr = ddin2MeasurementData.Report.TimeDiscr;
            ApertNumber = ddin2MeasurementData.ApertNumber;
            ModelPump = ddin2MeasurementData.ModelPump;

            if (ddin2MeasurementData.DynGraph != null)
            {
                DynGraph = new List<byte>(ddin2MeasurementData.DynGraph).ToArray();
            }
            if (ddin2MeasurementData.AccelerationGraph != null)
            {
                AccelerationGraph = new List<byte>(ddin2MeasurementData.AccelerationGraph).ToArray();
            }

            Field = ddin2MeasurementData.SecondaryParameters.Field;
            Well = ddin2MeasurementData.SecondaryParameters.Well;
            Bush = ddin2MeasurementData.SecondaryParameters.Bush;
            Shop = ddin2MeasurementData.SecondaryParameters.Shop;
            BufferPressure = ddin2MeasurementData.SecondaryParameters.BufferPressure;
            Comment = ddin2MeasurementData.SecondaryParameters.Comment;
            Name = ddin2MeasurementData.SecondaryParameters.DeviceName;

            Rod = ddin2MeasurementData.Rod / 10;

            DateTime = ddin2MeasurementData.Date;
            ErrorCode = ddin2MeasurementData.ErrorCode;
        }
    }
}
