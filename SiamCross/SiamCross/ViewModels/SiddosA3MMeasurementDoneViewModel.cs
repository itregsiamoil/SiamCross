﻿using SiamCross.DataBase.DataBaseModels;
using System.Collections.ObjectModel;

namespace SiamCross.ViewModels
{
    public class SiddosA3MMeasurementDoneViewModel : BaseViewModel, IViewModel
    {
        private SiddosA3MMeasurement _measurement;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField 
        {
            get => _measurement.Field;
            set => _measurement.Field = value; 
        }
        public string Well 
        {
            get => _measurement.Well;
            set => _measurement.Well = value;
        }
        public string Bush 
        {
            get => _measurement.Bush;
            set => _measurement.Bush = value;
        }
        public string Shop 
        {
            get => _measurement.Shop;
            set => _measurement.Shop = value;
        }
        public string Date { get; }
        public string BufferPressure 
        {
            get => _measurement.BufferPressure;
            set => _measurement.BufferPressure = value;
        }
        public string Comments 
        {
            get => _measurement.Comment;
            set => _measurement.Comment = value;
        }

        public string DeviceName { get; }

        public string MeasurementType { get; }
        public string ApertNumber 
        {
            get => _measurement.ApertNumber.ToString();
            set
            {
                if (short.TryParse(value, out short apertNumber))
                    _measurement.ApertNumber = apertNumber;
            }
        }

        public string MaxLoad { get;  }
        public string MinLoad { get;  }
        public string Imtravel { get;  }
        public string PumpRate { get;  }

        public string UpperRodWeight { get;  }
        public string LowerRodWeight { get;  }
        public string SelectedModelPump { get;  }

        public SiddosA3MMeasurementDoneViewModel(SiddosA3MMeasurement measurement)
        {
            _measurement = measurement;
            Fields = new ObservableCollection<string>()
            {
                "Первое поле",
                "Второе поле",
                "Третье поле"
            };

            InitDynGraph();

            SelectedField = _measurement.Field;
            Well = _measurement.Well;
            Bush = _measurement.Bush;
            Shop = _measurement.Shop;
            Date = _measurement.DateTime.ToString();
            BufferPressure = _measurement.BufferPressure;
            Comments = _measurement.Comment;
            DeviceName = _measurement.Name;
            MeasurementType = "Динамограмма";
            ApertNumber = _measurement.ApertNumber.ToString();
            MaxLoad = _measurement.MaxWeight.ToString();
            MinLoad = _measurement.MinWeight.ToString();
            Imtravel = _measurement.TravelLength.ToString();    //
            PumpRate = _measurement.SwingCount.ToString();      //
            UpperRodWeight = "0";
            LowerRodWeight = "0";
            switch (_measurement.ModelPump)
            {
                case 0:
                    SelectedModelPump = "Балансирный";
                    break;
                case 1:
                    SelectedModelPump = "Цепной";
                    break;
                case 2:
                    SelectedModelPump = "Гидравлический";
                    break;
                default:
                    break;
            }
        }

        private void InitDynGraph()
        {
            

            //var points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
            //    _measurement.Step,
            //    _measurement.WeightDiscr);

            //for (int i = 0; i < points.GetUpperBound(0); i++)
            //{
            //    //series.Points.Add(new DataPoint(points[i, 0], points[i, 1]));
            //    series.Points.Add(new DataPoint(i, points[i, 0]));
            //}

            
        }
    }
}