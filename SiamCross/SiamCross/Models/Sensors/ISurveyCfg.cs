using SiamCross.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurveyCfg : INotifyPropertyChanged
    {
        ICommand CmdSaveParam { get; }
        ICommand CmdLoadParam { get; }
        //ICommand CmdShow { get; }
        void ResetSaved();
    }
    public abstract class BaseSurveyCfg : BaseVM, ISurveyCfg
    {
        public ICommand CmdSaveParam { get; set; }
        public ICommand CmdLoadParam { get; set; }
        //public ICommand CmdShow { get; set; }

        public virtual void ResetSaved() { }
    }
}
