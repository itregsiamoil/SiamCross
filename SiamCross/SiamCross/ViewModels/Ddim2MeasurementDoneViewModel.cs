using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SiamCross.ViewModels
{
    public class Ddim2MeasurementDoneViewModel : BaseViewModel, IViewModel
    {
        private Ddim2Measurement _measurement;

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
    }
}
