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
        Task<bool> Execute(TaskManager mgr);
        Task Cancel();

    }

    public abstract class BaseTask : ITask
    {
        protected CancellationTokenSource _Cts = null;
        TaskManager _Mgr = null;
        string _Info;
        float _Progress;

        public TaskManager Manager
        {
            get => _Mgr;
            protected set => _Mgr = value;//_Mgr?.Manager.Report(_Mgr);
        }
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
        public async Task<bool> Execute(TaskManager mgr)
        {
            bool ret = false;
            try
            {
                _Cts = new CancellationTokenSource();
                Manager = mgr;
                ret = await DoExecute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION Execute lock"
                    + ex.Message + " " + ex.GetType() + "\n"
                    + ex.StackTrace + "\n");
                ret = true;
            }
            finally
            {
                _Cts.Dispose();
                Manager = null;
            }
            return ret;
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
