using Microsoft.VisualStudio.Threading;
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
        readonly AsyncReaderWriterLock _Lock;
        readonly Progress<ITask> _Task = new Progress<ITask>();
        readonly Progress<string> _Info = new Progress<string>();
        readonly Progress<float> _Progress = new Progress<float>();

        public TaskManager()
        {
            var taskContext = new JoinableTaskContext();
            var taskCollection = new JoinableTaskCollection(taskContext);
            JoinableTaskFactory taskFactory = taskContext.CreateFactory(taskCollection);
            _Lock = new AsyncReaderWriterLock(taskContext);
        }
        public IProgress<ITask> Task => _Task;
        public IProgress<string> Info => _Info;
        public IProgress<float> Progress => _Progress;
        public Progress<ITask> OnChangeTask => _Task;
        public Progress<string> OnChangeInfo => _Info;
        public Progress<float> OnChangeProgress => _Progress;
        protected async Task Subscribe(ITask task)
        {
            using (await _Lock.WriteLockAsync())
            {
                CurrentTask = task;
                Task.Report(task);
            }
        }
        protected async Task Unsubscribe()
        {
            using (await _Lock.WriteLockAsync())
            {
                if (null == CurrentTask)
                    return;
                Task.Report(null);
                CurrentTask = null;
            }
        }
        public async Task<bool> Execute(ITask task)
        {
            bool ret = false;
            if (null == task)
                return ret;
            try
            {
                using (await _Lock.UpgradeableReadLockAsync())
                {
                    if (null != CurrentTask)
                        return false;
                    await Subscribe(task).ConfigureAwait(false);
                }
                ret = await task.Execute(this).ConfigureAwait(false);
                await Unsubscribe().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION Execute lock"
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
            using (await _Lock.ReadLockAsync())
                await CurrentTask?.Cancel();
        }
        public async Task RefreshTask()
        {
            using (await _Lock.ReadLockAsync())
                Task.Report(CurrentTask);
        }
    }
}
