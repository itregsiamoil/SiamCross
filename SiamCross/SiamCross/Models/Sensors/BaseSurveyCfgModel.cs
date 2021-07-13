using SiamCross.ViewModels;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSurveyCfgModel : BaseVM
    {
        public ITask TaskSave { get; set; }
        public ITask TaskLoad { get; set; }
        public virtual void ResetSaved() { }
        public virtual void UpdateSaved() { }
        public abstract bool IsSync();
    }
}
