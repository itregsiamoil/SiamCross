using System.Collections.ObjectModel;

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
