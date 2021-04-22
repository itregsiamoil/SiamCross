using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public enum JobStatus
    {
        Created = -2,
        Started = -1,
        Сomplete = 0,
        Canceled,
        Terminated,
        Error,
    }
    public interface ITask
    {
        TaskManager Manager { get; }
        string Info { get; }
        float Progress { get; }
        Task<JobStatus> ExecAsync(TaskManager mgr, CancellationToken ct);
        Task CancelAsync();

        JobStatus Status { get; }
    }

    public abstract class BaseTask : ITask
    {
        TaskManager _Mgr = null;
        string _Info;
        float _Progress;
        string _Name;
        JobStatus _Status = JobStatus.Created;
        public JobStatus Status => _Status;

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
                else if (0f > value)
                {
                    if (0f == _Progress)
                        return;
                    _Progress = 0f;
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


        public async Task<JobStatus> ExecAsync(TaskManager mgr, CancellationToken ct)
        {
            _Status = JobStatus.Started;
            if (null == mgr)
            {
                _Status = JobStatus.Error;
                return _Status;
            }

            try
            {
                Manager = mgr;
                Progress = 0.00f;
                Manager.Progress.Report(_Progress);
                if (await DoExecuteAsync(ct))
                {
                    Progress = 1f;
                    _Status = JobStatus.Сomplete;
                }
                else
                    switch (_Status)
                    {
                        default:
                            _Status = ct.IsCancellationRequested ?
                            JobStatus.Terminated : JobStatus.Error;
                            break;
                        case JobStatus.Canceled: break;
                    }
            }
            catch (OperationCanceledException)
            {
                switch (_Status)
                {
                    default:
                        _Status = ct.IsCancellationRequested ?
                        JobStatus.Terminated : JobStatus.Error;
                        break;
                    case JobStatus.Canceled: break;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                _Status = JobStatus.Error;
            }
            finally
            {
                Manager = null;
            }
            return _Status;
        }
        public abstract Task<bool> DoExecuteAsync(CancellationToken ct);

        public virtual Task DoBeforeCancelAsync()
        {
            return Task.CompletedTask;
        }
        public async Task CancelAsync()
        {
            try
            {
                await DoBeforeCancelAsync();
                _Status = JobStatus.Canceled;
            }
            catch (Exception ex)
            {
                LogException(ex);
                _Status = JobStatus.Error;
            }
        }
        public static void LogException(Exception ex)
        {
            Debug.WriteLine("EXCEPTION: "
                + "\n TYPE=" + ex.GetType()
                + "\n MESSAGE=" + ex.Message
                + "\n STACK=" + ex.StackTrace + "\n");
        }

    }
}
