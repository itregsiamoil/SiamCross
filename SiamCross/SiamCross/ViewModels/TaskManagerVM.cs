using SiamCross.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels
{
    public class TaskManagerVM : BasePageVM, IDisposable
    {
        TaskManager _Model;
        void SetHidden(object obj, bool val)
        {
            IsHidden = val;
            ChangeNotify(nameof(IsHidden));
        }
        void SetInfo(object obj, string info)
        {
            Info = info;
            ChangeNotify(nameof(Info));
        }
        void SetProgress(object obj, float progress)
        {
            Progress = progress;
            ChangeNotify(nameof(Progress));
            ChangeNotify(nameof(ProgressInt));
        }
        void SetTask(object obj, ITask task)
        {
            IsFree = null == task;
            IsBusy = !IsFree;
            ChangeNotify(nameof(IsFree));
            ChangeNotify(nameof(IsBusy));
        }
        void Subscribe(TaskManager model)
        {
            _Model = model;
            _Model.OnChangeTask.ProgressChanged += SetTask;
            _Model.OnChangeInfo.ProgressChanged += SetInfo;
            _Model.OnChangeProgress.ProgressChanged += SetProgress;
            _Model.OnChangeHidden.ProgressChanged += SetHidden;
            _Model.RefreshTask().ConfigureAwait(false);
        }
        public override void Unsubscribe()
        {
            _Model.OnChangeTask.ProgressChanged -= SetTask;
            _Model.OnChangeInfo.ProgressChanged -= SetInfo;
            _Model.OnChangeProgress.ProgressChanged -= SetProgress;
            _Model.OnChangeHidden.ProgressChanged -= SetHidden;
        }


        public TaskManager GetModel() { return _Model; }
        public string Info { get; protected set; }
        public float Progress { get; protected set; }
        public bool IsBusy { get; protected set; }
        public bool IsFree { get; protected set; }
        public int ProgressInt => (int)(100 * Progress);
        public bool IsHidden { get; protected set; }

        public ICommand CancelCmd { get; }

        async Task DoCancel()
        {
            await _Model.Cancel();
        }
        public TaskManagerVM(TaskManager model)
        {
            IsHidden = true;
            Subscribe(model);
            CancelCmd = new AsyncCommand(DoCancel, () => IsBusy, null, false, false);
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
