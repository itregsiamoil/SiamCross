using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SiamCross.ViewModels
{
    public class DuMeasurementViewModel: BaseViewModel, IViewModel
    {
        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comments { get; set; }
        public string DynPeriod { get; set; }
        public string ApertNumber { get; set; }
        public string Imtravel { get; set; }
    }
}
