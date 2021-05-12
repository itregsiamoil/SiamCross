using SiamCross.ViewModels;
using System.ComponentModel;

namespace SiamCross.Models.Sensors
{
    public interface ISurveyCfg : INotifyPropertyChanged
    {
        ITask TaskSave { get; set; }
        ITask TaskLoad { get; set; }
        ITask TaskWait { get; set; }
        void ResetSaved();
        void UpdateSaved();
    }
    public abstract class BaseSurveyCfg : BaseVM, ISurveyCfg
    {
        public ITask TaskSave { get; set; }
        public ITask TaskLoad { get; set; }
        public ITask TaskWait { get; set; }
        public virtual void ResetSaved() { }
        public virtual void UpdateSaved() { }
    }
}
