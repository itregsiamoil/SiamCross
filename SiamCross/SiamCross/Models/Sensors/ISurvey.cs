using SiamCross.ViewModels;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurvey
    {
        ICommand CmdStart { get; set; }
        ICommand CmdSaveParam { get; set; }
        ICommand CmdLoadParam { get; set; }
    }
    public class BaseSurvey : BaseVM, ISurvey
    {
        public string Name;
        public string Description;
        public ICommand CmdStart { get; set; }
        public ICommand CmdSaveParam { get; set; }
        public ICommand CmdLoadParam { get; set; }
    }

}
