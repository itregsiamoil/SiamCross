using SiamCross.Models.Sensors;
using System.Windows.Input;

namespace SiamCross.ViewModels
{
    public abstract class BaseStorageVM : BasePageVM
    {
        public BaseStorageModel Model { get; }
        public ICommand CmdUpdateStorageInfo { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdClearStorage { get; set; }

        public BaseStorageVM(BaseStorageModel model)
        {
            Model = model;
            CmdUpdateStorageInfo = Model?.CmdUpdateStorageInfo;
            CmdDownload = Model?.CmdDownload;
            CmdClearStorage = Model?.CmdClearStorage;
        }
    }
}
