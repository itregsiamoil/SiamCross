using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class TaskManager
    {
        ReaderWriterLockSlim _QueueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        //Queue<ITask> _Tasks = new Queue<ITask>();
        //SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public ITask CurrentTask { get; protected set; }

        public event Action<TaskManager> OnChangeManager;
        public event Action<string> OnChangeInfo;
        public event Action<float> OnChangeProgress;

        protected void Subscribe(ITask task)
        {
            CurrentTask = task;
            CurrentTask.OnChangeManager += OnChangeManager;
            CurrentTask.OnChangeInfo += OnChangeInfo;
            CurrentTask.OnChangeProgress += OnChangeProgress;
        }
        protected void Unsubscribe()
        {
            CurrentTask.OnChangeManager -= OnChangeManager;
            CurrentTask.OnChangeInfo -= OnChangeInfo;
            CurrentTask.OnChangeProgress -= OnChangeProgress;
            CurrentTask = null;
        }

        public async Task<bool> Execute(ITask task)
        {
            if (null == task)
                return false;
            bool res = false;
            _QueueLock.EnterUpgradeableReadLock();
            try
            {
                if (null != CurrentTask)
                    return false;

                _QueueLock.EnterWriteLock();
                try
                {
                    Subscribe(task);
                    res = await task.Execute(this);
                    if (res)
                        OnChangeInfo?.Invoke($"\u2713 {task.Info}");
                    else
                        OnChangeInfo?.Invoke($"\u2716 {task.Info}");
                    return res;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EXCEPTION Execute task"
                    + ex.Message + " "
                    + ex.GetType() + " "
                    + ex.StackTrace + "\n");
                }
                finally
                {
                    Unsubscribe();
                    _QueueLock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION Execute lock"
                    + ex.Message + " "
                    + ex.GetType() + " "
                    + ex.StackTrace + "\n");
                res = false;
            }
            finally
            {
                _QueueLock.ExitUpgradeableReadLock();
            }
            return res;
            //using (await semaphore.UseWaitAsync())
        }
        public async Task Cancel()
        {
            _QueueLock.EnterReadLock();
            await CurrentTask?.Cancel();
        }

    }
}
