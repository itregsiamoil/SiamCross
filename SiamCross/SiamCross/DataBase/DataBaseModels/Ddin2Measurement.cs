using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.Models.Sensors.Ddim2.Measurement;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SQLite;

namespace SiamCross.DataBase.DataBaseModels
{
    [Table("Ddin2Measurement")]
    public class Ddin2Measurement
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        /*/ Report /*/
        public short MaxWeight { get; set; }
        public short MinWeight { get; set; }
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

        public Ddin2Measurement() { }
        public Ddin2Measurement(Ddin2MeasurementData ddin2MeasurementData)
        {
            MaxWeight = ddin2MeasurementData.Report.MaxWeight;
            MinWeight = ddin2MeasurementData.Report.MinWeight;
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

            DateTime = ddin2MeasurementData.Date;
            ErrorCode = ddin2MeasurementData.ErrorCode;
        }
    }
}
