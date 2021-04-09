using SiamCross.ViewModels;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurvey
    {
        ICommand CmdStart { get; set; }
        ICommand CmdCancel { get; set; }
        ICommand CmdUpdate{ get; set; }
    }
    public class BaseSurvey : BaseVM, ISurvey
    {
        public string Name;
        public string Description;
        public ICommand CmdStart { get; set; }
        public ICommand CmdCancel { get; set; }
        public ICommand CmdUpdate { get; set; }
    }
    
}
