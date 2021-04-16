using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class TaskManager : IDisposable
    {
        readonly bool IsBackground = Thread.CurrentThread.IsBackground;
        readonly int ThreadId = Thread.CurrentThread.ManagedThreadId;
        ITask CurrentTask = null;
        CancellationTokenSource _TaskCts = new CancellationTokenSource();
        private readonly SemaphoreSlim _Lock = new SemaphoreSlim(1);
        readonly Progress<ITask> _Task = new Progress<ITask>();
        readonly Progress<string> _Info = new Progress<string>();
        readonly Progress<float> _Progress = new Progress<float>();
        readonly Progress<bool> _Hidden = new Progress<bool>();
        readonly Timer _VisibleTimer;

        public TaskManager()
        {
            _VisibleTimer = new Timer(new TimerCallback(OnTimer), null, Timeout.Infinite, 0);
        }
        public IProgress<ITask> Task => _Task;
        public IProgress<string> Info => _Info;
        public IProgress<float> Progress => _Progress;
        public Progress<ITask> OnChangeTask => _Task;
        public Progress<string> OnChangeInfo => _Info;
        public Progress<float> OnChangeProgress => _Progress;
        public Progress<bool> OnChangeHidden => _Hidden;

        protected void Subscribe(ITask task)
        {
            Interlocked.Exchange(ref CurrentTask, task);
            Task.Report(CurrentTask);
            (_Hidden as IProgress<bool>).Report(false);
        }

        protected void Unsubscribe()
        {
            if (null == CurrentTask)
                return;
            Interlocked.Exchange(ref CurrentTask, null);
            Task.Report(null);
            _VisibleTimer.Change(10000, 0);
        }
        public async Task<bool> Execute(ITask task)
        {
            bool ret = false;
            if (null == task)
                return ret;
            try
            {
                using (await _Lock.UseWaitAsync())
                {
                    if (null != CurrentTask)
                        return false;
                    Subscribe(task);
                    if (_TaskCts.IsCancellationRequested)
                    {
                        _TaskCts.Dispose();
                        _TaskCts = new CancellationTokenSource();
                    }
                }
                ret = await task.ExecAsync(this, _TaskCts.Token);
                using (await _Lock.UseWaitAsync())
                    Unsubscribe();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION "
                    + ex.Message + " " + ex.GetType() + "\n"
                    + ex.StackTrace + "\n");
                ret = false;
            }
            if (ret)
                Info?.Report($"\u2713 {task.Info}");
            else
                Info?.Report($"\u2716 {task.Info}");
            return ret;
        }
        public async Task Cancel()
        {
            using (await _Lock.UseWaitAsync())
            {
                if (null != CurrentTask)
                {
                    await CurrentTask.CancelAsync();
                    _TaskCts.Cancel();
                }
            }
        }
        public async Task RefreshTask()
        {
            using (await _Lock.UseWaitAsync())
                Task.Report(CurrentTask);
        }
        void OnTimer(object obj)
        {
            if (null == CurrentTask)
            {
                (_Hidden as IProgress<bool>).Report(true);
                _VisibleTimer.Change(Timeout.Infinite, 0);
            }
        }
        public void Dispose()
        {
            _TaskCts.Cancel();
            _TaskCts.Dispose();
            _VisibleTimer.Dispose();
            _Lock.Dispose();
        }
    }
}
