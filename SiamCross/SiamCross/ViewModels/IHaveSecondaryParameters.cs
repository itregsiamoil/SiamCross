using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.ViewModels
{
    public interface IHaveSecondaryParameters
    {
        ObservableCollection<string> Fields { get; set; }
        string SelectedField { get; set; }
        string Well { get; set; }
        string Bush { get; set; }
        string Shop { get; set; }
        string BufferPressure { get; set; }
        string Comments { get; set; }
    }
}
