using SiamCross.ViewModels;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseStorageModel : BaseVM
    {
        public ICommand CmdUpdateStorageInfo { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdClearStorage { get; set; }
    }
}
