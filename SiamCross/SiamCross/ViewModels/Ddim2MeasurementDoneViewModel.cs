﻿using SiamCross.DataBase.DataBaseModels;
using Microcharts;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SiamCross.ViewModels
{
    public class Ddim2MeasurementDoneViewModel : BaseViewModel, IViewModel
    {
        private Ddim2Measurement _measurement;

        public LineChart DynGraph { get; set; }
        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string Date { get; private set; }
        public string BufferPressure { get; set; }
        public string Comments { get; set; }

        public string DeviceName { get; set; }

        public string MeasurementType { get; set; }
        public string ApertNumber { get; set; }

        public string MaxLoad { get; set; }
        public string MinLoad { get; set; }
        public string Imtravel { get; set; }
        public string PumpRate { get; set; }

        public string UpperRodWeight { get; set; }
        public string LowerRodWeight { get; set; }
        public ObservableCollection<string> ModelPump { get; set; }
        public string SelectedModelPump { get; set; }

        public Ddim2MeasurementDoneViewModel(Ddim2Measurement measurement)
        {
            _measurement = measurement;
            Fields = new ObservableCollection<string>()
            {
                "Первое поле",
                "Второе поле",
                "Третье поле"
            };
            ModelPump = new ObservableCollection<string>()
            {
                "Балансирный",
                "Цепной",
                "Гидравлический"
            };

            InitDynGraph();

            SelectedField = _measurement.Field;
            Well = _measurement.Well;
            Bush = _measurement.Bush;
            Shop = _measurement.Shop;
            Date = _measurement.DateTime.ToString();
            BufferPressure = _measurement.BufferPressure;
            Comments = _measurement.Comment;
            DeviceName = "DDIM2";
            MeasurementType = "Динамограмма";
            ApertNumber = "1";
            MaxLoad = _measurement.MaxWeight.ToString();
            MinLoad = _measurement.MinWeight.ToString();
            Imtravel = _measurement.Travel.ToString();
            PumpRate = "120";
            UpperRodWeight = "0";
            LowerRodWeight = "0";
        }

        private void InitDynGraph()
        {


            var points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                _measurement.Step,
                _measurement.WeightDiscr);

            Entry[] entries = new Entry[points.GetUpperBound(0)];

            for (int i = 0; i < points.GetUpperBound(0); i++)
            {
                //series.Points.Add(new DataPoint(points[i, 0], points[i, 1]));
                //.Points.Add(new DataPoint(i, points[i, 0]));
                entries[i] = new Entry((float)points[i, 0])
                {
                    Label = i.ToString()
                };
            }

            DynGraph = new LineChart() { Entries = entries };
        }
    }
}
