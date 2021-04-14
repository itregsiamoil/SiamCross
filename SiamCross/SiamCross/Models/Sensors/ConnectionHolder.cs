using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public class ConnectionHolder : ViewModels.BaseVM, IDisposable
    {
        readonly Timer _AliveTimer;
        DateTime _LastExchange = DateTime.Now;
        bool _Runed = false;
        CancellationTokenSource _cancellToken = null;

        bool _IsManagerBusy;
        readonly TaskManager _Manager;
        readonly Connection.IConnection _Connection;

        Func<CancellationToken, Task> _QuckReportFn;


        bool _IsQickInfo;
        bool _IsActivated;

        public bool IsQickInfo { get => _IsQickInfo; set => SetProperty(ref _IsQickInfo, value); }
        public bool IsOnline => _Connection.State == Connection.ConnectionState.Connected;
        public bool IsActivated
        {
            get => _IsActivated;
            set
            {
                _IsActivated = value;
                ChangeNotify();
                if (_IsActivated)
                {
                    if (null != _cancellToken)
                        _cancellToken.Dispose();
                    _cancellToken = new CancellationTokenSource();
                    _AliveTimer.Change(0, 1000);
                }
                else
                {
                    _cancellToken?.Cancel();
                }

            }
        }

        public ConnectionHolder(TaskManager mgr, Connection.IConnection connection
            , Func<CancellationToken, Task> quckReportFn = null)
        {
            _QuckReportFn = quckReportFn;
            _Connection = connection;
            _Connection.PropertyChanged += OnConnectionChange;

            _Manager = mgr;
            _Manager.OnChangeTask.ProgressChanged += OnManegerChangeTask;

            _AliveTimer = new Timer(new TimerCallback(OnTimer), null, System.Threading.Timeout.Infinite, 1000);
        }

        void OnTimer(object obj)
        {
            if (!_Runed)
            {
                _Runed = true;
                Task.Run(DoSingle);
            }
        }
        async Task DoSingle()
        {
            try
            {
                if (null != _cancellToken && !_cancellToken.IsCancellationRequested)
                    await DoLive();
                else
                    await DoDisableLive();
            }
            catch (OperationCanceledException)
            {
                await DoDisableLive();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION in LiveUpdate"
                + ex.Message + " "
                + ex.GetType() + " "
                + ex.StackTrace + "\n");
            }
            finally
            {
                //if(_cancellToken.Token.IsCancellationRequested)
                _Runed = false;
            }
        }
        async Task DoLive()
        {
            _cancellToken.Token.ThrowIfCancellationRequested();
            if (IsOnline)
            {
                if (_IsManagerBusy)
                {
                    _LastExchange = DateTime.Now;
                    return;
                }
                if (_IsQickInfo || 30 < (DateTime.Now - _LastExchange).TotalSeconds)
                {
                    await _QuckReportFn?.Invoke(_cancellToken.Token);
                    _LastExchange = DateTime.Now;
                    //_Connection.PhyConnection.UpdateRssi();
                }
            }
            else
            {
                _cancellToken.Token.ThrowIfCancellationRequested();
                await _Connection.Connect(_cancellToken.Token);
                Debug.WriteLine(IsOnline ? "StartAlive OK" : "StartAlive FAILED");
            }
        }
        async Task DoDisableLive()
        {
            _AliveTimer.Change(System.Threading.Timeout.Infinite, 1000);
            await _Connection.Disconnect();
            Debug.WriteLine($"Cancel liveupdate");
        }
        void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            if (sender.Equals(_Connection) && e.PropertyName == nameof(Connection.IConnection.State))
                ChangeNotify(nameof(IsOnline));
        }
        void OnManegerChangeTask(object obj, ITask task)
        {
            _IsManagerBusy = null != task; ;
        }
        public void Dispose()
        {
            IsActivated = false;
            _AliveTimer?.Dispose();
            _Connection.PropertyChanged -= OnConnectionChange;
            _Manager.OnChangeTask.ProgressChanged -= OnManegerChangeTask;
            _cancellToken?.Dispose();
        }
    }
}
