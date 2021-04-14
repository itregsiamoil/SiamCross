using SiamCross.Models.Tools;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class TaskManager
    {
        readonly bool IsBackground = Thread.CurrentThread.IsBackground;
        readonly int ThreadId = Thread.CurrentThread.ManagedThreadId;
        ITask CurrentTask = null;
        private readonly SemaphoreSlim _Lock = new SemaphoreSlim(1);
        readonly Progress<ITask> _Task = new Progress<ITask>();
        readonly Progress<string> _Info = new Progress<string>();
        readonly Progress<float> _Progress = new Progress<float>();

        public TaskManager()
        {
        }
        public IProgress<ITask> Task => _Task;
        public IProgress<string> Info => _Info;
        public IProgress<float> Progress => _Progress;
        public Progress<ITask> OnChangeTask => _Task;
        public Progress<string> OnChangeInfo => _Info;
        public Progress<float> OnChangeProgress => _Progress;
        protected void Subscribe(ITask task)
        {
            CurrentTask = task;
            Task.Report(task);
        }

        protected void Unsubscribe()
        {
            if (null == CurrentTask)
                return;
            Task.Report(null);
            CurrentTask = null;
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
                }
                ret = await task.ExecAsync(this);
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
                await CurrentTask?.CancelAsync();
        }
        public async Task RefreshTask()
        {
            using (await _Lock.UseWaitAsync())
                Task.Report(CurrentTask);
        }
    }
}
