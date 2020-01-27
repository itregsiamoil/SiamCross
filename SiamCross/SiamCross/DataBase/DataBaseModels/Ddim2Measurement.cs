using SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement;
using System;
using System.Collections.Generic;
using System.Text;
namespace SiamCross.DataBase.DataBaseModels
{
    public class Ddim2Measurement
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

        public Ddim2Measurement() { }
        public Ddim2Measurement(Ddim2MeasurementData ddim2MeasurementData)
        {
            TravelLength = ddim2MeasurementData.Report.Travel * 
                ddim2MeasurementData.Report.Step / 10000;

            SwingCount = (60 / 0.001) / (ddim2MeasurementData.Report.Period * 
                ddim2MeasurementData.Report.TimeDiscr);

            MaxWeight = ddim2MeasurementData.Report.WeightDiscr * 
                ddim2MeasurementData.Report.MaxWeight / 1000f;
            MinWeight = ddim2MeasurementData.Report.WeightDiscr * 
                ddim2MeasurementData.Report.MinWeight / 1000f;

            Travel = ddim2MeasurementData.Report.Travel;
            Period = ddim2MeasurementData.Report.Period;
            Step = ddim2MeasurementData.Report.Step;
            WeightDiscr = ddim2MeasurementData.Report.WeightDiscr;
            TimeDiscr = ddim2MeasurementData.Report.TimeDiscr;
            ApertNumber = ddim2MeasurementData.ApertNumber;
            ModelPump = ddim2MeasurementData.ModelPump;

            if (ddim2MeasurementData.DynGraph != null)
            {
                DynGraph = new List<byte>(ddim2MeasurementData.DynGraph).ToArray();
            }
            if (ddim2MeasurementData.AccelerationGraph != null)
            {
                AccelerationGraph = new List<byte>(ddim2MeasurementData.AccelerationGraph).ToArray();
            }

            Field = ddim2MeasurementData.SecondaryParameters.Field;
            Well = ddim2MeasurementData.SecondaryParameters.Well;
            Bush = ddim2MeasurementData.SecondaryParameters.Bush;
            Shop = ddim2MeasurementData.SecondaryParameters.Shop;
            BufferPressure = ddim2MeasurementData.SecondaryParameters.BufferPressure;
            Comment = ddim2MeasurementData.SecondaryParameters.Comment;
            Name = ddim2MeasurementData.SecondaryParameters.DeviceName;

            MaxBarbellWeight = 0;
            MinBarbellWeight = 0;

            DateTime = ddim2MeasurementData.Date;
            ErrorCode = ddim2MeasurementData.ErrorCode;
        }
    }
}
