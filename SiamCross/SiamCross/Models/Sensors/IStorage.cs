using SiamCross.ViewModels;
using System;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface IStorage
    {
        ICommand CmdUpdateStorageInfo { get; set; }
        ICommand CmdDownload { get; set; }
        ICommand CmdClearStorage { get; set; }
    }

    public class BaseStorage : BaseVM, IStorage
    {
        public ICommand CmdUpdateStorageInfo { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdClearStorage { get; set; }
    }

    public abstract class BaseStorageVM : BaseVM, IStorage, IDisposable
    {
        public IStorage Model { get; }
        public ICommand CmdUpdateStorageInfo { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdClearStorage { get; set; }

        public BaseStorageVM(IStorage model)
        {
            Model = model;
            CmdUpdateStorageInfo = Model?.CmdUpdateStorageInfo;
            CmdDownload = Model?.CmdDownload;
            CmdClearStorage = Model?.CmdClearStorage;
        }

        public abstract void Dispose();
    }
}
