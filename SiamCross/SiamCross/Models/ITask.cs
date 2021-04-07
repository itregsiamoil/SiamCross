using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface ITask
    {
        TaskManager Manager { get; }
        string Info { get; }
        float Progress { get; }
        Task<bool> ExecAsync(TaskManager mgr);
        Task CancelAsync();

    }

    public abstract class BaseTask : ITask
    {
        protected CancellationTokenSource _Cts = null;
        TaskManager _Mgr = null;
        string _Info;
        float _Progress;
        private string _Name;

        public TaskManager Manager
        {
            get => _Mgr;
            protected set => _Mgr = value;//_Mgr?.Manager.Report(_Mgr);
        }

        protected string Name { get => _Name; set => _Name = value; }
        public string InfoEx { set => Info = $"{Name}:\n{value}"; }

        public string Info
        {
            get => _Info;
            set
            {
                _Info = value;
                _Mgr?.Info.Report(_Info);
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
                _Mgr?.Progress.Report(_Progress);
            }

        }


        public async Task<bool> ExecAsync(TaskManager mgr)
        {
            if (null == mgr)
                return false;
            bool ret = false;
            _Cts = new CancellationTokenSource();
            try
            {
                Progress = 0f;
                Manager = mgr;
                ret = await DoExecute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION "
                    + ex.Message + " " + ex.GetType() + "\n"
                    + ex.StackTrace + "\n");
                ret = false;
            }
            finally
            {
                Progress = 1f;
                _Cts?.Dispose();
                Manager = null;
            }
            return ret;
        }
        public abstract Task<bool> DoExecute();

        public virtual Task DoBeforeCancelAsync()
        {
            return Task.CompletedTask;
        }
        public async Task CancelAsync()
        {
            await DoBeforeCancelAsync();
            _Cts?.Cancel();
        }

    }
}
