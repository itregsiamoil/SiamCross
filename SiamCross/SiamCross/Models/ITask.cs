using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface ITask
    {
        event Action<TaskManager> OnChangeManager;
        event Action<string> OnChangeInfo;
        event Action<float> OnChangeProgress;

        TaskManager Manager { get; }
        string Info { get; }
        float Progress { get; }
        Task<bool> Execute(TaskManager mgr);
        Task Cancel();

    }

    public abstract class BaseTask : ITask
    {
        protected CancellationTokenSource _Cts = null;
        TaskManager _Mgr = null;
        string _Info;
        float _Progress;

        public event Action<TaskManager> OnChangeManager;
        public event Action<string> OnChangeInfo;
        public event Action<float> OnChangeProgress;

        public TaskManager Manager
        {
            get => _Mgr;
            protected set
            {
                _Mgr = value;
                OnChangeManager?.Invoke(_Mgr);
            }
        }
        public string Info
        {
            get => _Info;
            set
            {
                _Info = value;
                OnChangeInfo?.Invoke(_Info);
            }
        }
        public float Progress
        {
            get => _Progress;
            set
            {
                if (1.0f < value)
                {
                    if (1.0f == _Progress)
                        return;
                    _Progress = 1.0f;
                }
                else
                {
                    if (_Progress == value)
                        return;
                    _Progress = value;
                }
                OnChangeProgress?.Invoke(_Progress);
            }

        }
        public async Task<bool> Execute(TaskManager mgr)
        {
            try
            {
                _Cts = new CancellationTokenSource();
                Manager = mgr;
                return await DoExecute();
            }
            catch (Exception)
            {
            }
            finally
            {
                _Cts.Dispose();
                _Cts = null;
                //Progress = 1;
                //Info = "End";
                Manager = null;
            }
            return false;
        }
        public async Task Cancel()
        {
            _Cts?.Cancel();
            await DoCancel();
            //return Task.CompletedTask;
        }


        public abstract Task<bool> DoExecute();
        public virtual Task DoCancel()
        {
            return Task.CompletedTask;
        }

    }
}
