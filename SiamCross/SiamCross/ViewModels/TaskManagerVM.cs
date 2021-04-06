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
            _Model.RefreshTask().ConfigureAwait(false);
        }
        void Unsubscribe()
        {
            _Model.OnChangeTask.ProgressChanged -= SetTask;
            _Model.OnChangeInfo.ProgressChanged -= SetInfo;
            _Model.OnChangeProgress.ProgressChanged -= SetProgress;
        }


        public TaskManager GetModel() { return _Model; }
        public string Info { get; protected set; }
        public float Progress { get; protected set; }
        public bool IsBusy { get; protected set; }
        public bool IsFree { get; protected set; }
        public int ProgressInt => (int)(100 * Progress);

        public ICommand CancelCmd { get; }

        async Task DoCancel()
        {
            await _Model.Cancel();
        }
        public TaskManagerVM(TaskManager model)
        {
            Subscribe(model);
            CancelCmd = new AsyncCommand(DoCancel, () => IsBusy, null, false, false);
        }
        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
