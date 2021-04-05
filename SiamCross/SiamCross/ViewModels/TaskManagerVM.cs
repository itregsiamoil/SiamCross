using SiamCross.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels
{
    public class TaskManagerVM : BaseVM, IDisposable
    {
        TaskManager _Model;
        void SetInfo(string info)
        {
            Info = info;
            ChangeNotify(nameof(Info));
        }
        void SetProgress(float progress)
        {
            Progress = progress;
            ChangeNotify(nameof(Progress));
            ChangeNotify(nameof(ProgressInt));
        }
        void SetManager(TaskManager mgr)
        {
            IsFree = null == mgr;
            IsBusy = !IsFree;
            ChangeNotify(nameof(IsFree));
            ChangeNotify(nameof(IsBusy));
        }
        void Subscribe(TaskManager model)
        {
            _Model = model;
            _Model.OnChangeManager += SetManager;
            _Model.OnChangeInfo += SetInfo;
            _Model.OnChangeProgress += SetProgress;
        }
        void Unsubscribe()
        {
            _Model.OnChangeManager -= SetManager;
            _Model.OnChangeInfo -= SetInfo;
            _Model.OnChangeProgress -= SetProgress;
        }

        public TaskManager GetModel() { return _Model; }
        public string Info { get; protected set; }
        public float Progress { get; protected set; }
        public bool IsBusy { get; protected set; }
        public bool IsFree { get; protected set; }
        public int ProgressInt => (int)(100 * Progress);

        public ICommand CancelCmd { get; set; }


        bool CanDoCancel()
        {
            if (null == _Model)
                return false;
            return null != _Model.CurrentTask;
        }
        async Task DoCancel()
        {
            await _Model?.CurrentTask?.Cancel();
        }
        public TaskManagerVM(TaskManager model)
        {
            IsFree = null == model.CurrentTask;
            IsBusy = !IsFree;
            Subscribe(model);
            CancelCmd = new AsyncCommand(DoCancel, CanDoCancel, null, false, false);
        }
        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
