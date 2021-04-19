#define DEBUG_UNIT
using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSensor : BaseVM, ISensor
    {
        readonly SensorModel _Model;
        public SensorModel Model => _Model;
        #region TmpVariables
        #endregion
        #region Constructors & Destructors
        //TaskScheduler _uiScheduler;

        public virtual void OnConnect()
        {

        }
        void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            if (null == sender || "State" != e.PropertyName)
                return;
            ChangeNotify(nameof(ConnStateStr));
            if (ConnectionState.Connected == Connection.State)
                OnConnect();
        }
        protected BaseSensor(IProtocolConnection conn, DeviceInfo deviceInfo)
        {
            _Model = new SensorModel(conn, deviceInfo);
            ScannedDeviceInfo = new ScannedDeviceInfo(_Model.Device);
            Firmware = "";
            Battery = "";
            Temperature = "";
            RadioFirmware = "";
            Status = "";

            IsMeasurement = false;
            // Получение планировщика UI для потока, который создал форму:
            //_uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Connection.PropertyChanged += OnConnectionChange;
            //mConnection.PropertyChanged += PropertyChanged;

            Connection.MaxReqLen = ScannedDeviceInfo.GetPrefferedPkgSize();

            ShowDetailViewCommand = PageNavigator.CreateAsyncCommand(() =>
            {
                //this.Activate = true;
                return new SensorDetailsVM(this);
            });
            TaskManager = new TaskManagerVM(_Model.Manager);

            SurveysVM = new SurveysCollectionVM(this);

            PositionVM = new PositionVM(this);

            Model.ConnHolder = new ConnectionHolder(
                Model.Manager, Connection, QuickReport);

        }

        public virtual async void Dispose()
        {
            Connection.PropertyChanged -= OnConnectionChange;
            await Deactivate();
            Model?.Dispose();
        }
        #endregion
        #region basic implementation
        #region Variables
        #endregion
        public string ConnStateStr => ConnectionStateAdapter.ToString(Connection.State);
        public int MeasureProgressP => (int)(mMeasureProgress * 100);

        private float mMeasureProgress = 0;

        public float MeasureProgress
        {
            get => mMeasureProgress;
            set
            {
                mMeasureProgress = value;
                ChangeNotify(nameof(MeasureProgress));
                ChangeNotify(nameof(MeasureProgressP));
            }
        }
        public IProtocolConnection Connection => Model.Connection;

        private bool mIsAlive;

        private bool mIsMeasurement = false;
        public bool IsMeasurement
        {
            get => mIsMeasurement;
            protected set
            {
                mIsMeasurement = value;
                ChangeNotify();
            }
        }
        public bool IsEnableQickInfo
        {
            get => Model.ConnHolder.IsQickInfo;
            set
            {
                Model.ConnHolder.IsQickInfo = value;
                ChangeNotify();
            }
        }
        public PositionVM PositionVM { get; }
        public ICommand ShowDetailViewCommand { get; set; }
        public ICommand ShowInfoViewCommand { get; set; }

        public BaseStorageVM StorageVM { get; set; }
        public IViewModel FactoryConfigVM { get; set; }
        public IViewModel UserConfigVM { get; set; }
        public IViewModel StateVM { get; set; }
        public SurveysCollectionVM SurveysVM { get; }


        public TaskManagerVM TaskManager { get; set; }

        public ICommand ShowSurveysViewCommand { get; set; }

        //public IReadOnlyList<SurveyVM> Surveys { get; set; }

        public ScannedDeviceInfo ScannedDeviceInfo { get; }
        public abstract Task<bool> QuickReport(CancellationToken cancellationToken);
        //public virtual Task<bool> UpdateRssi(CancellationToken cancellationToken);
        public abstract Task StartMeasurement(object measurementParameters);

        #endregion
        #region Activate implementation
        #region Variables

        private CancellationTokenSource _cancellToken = null;

        //TaskCompletionSource<bool> mLiveTaskCompleated=null;
        private Task _liveTask = null;
        private bool _activated = false;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        #endregion

        public async Task<bool> DoActivate(CancellationToken ct = default)
        {
            bool connected = false;
            for (int i = 0; i < Connection.Retry
                            && !connected
                            && !ct.IsCancellationRequested; ++i)
                connected = await Connection.Connect(ct);
            if (connected && !Activate)
                Activate = true;
            return connected;
        }

        private async Task AsyncActivate()
        {
            using (await semaphore.UseWaitAsync())
            {
                CancellationTokenSource ct = null;
                try
                {
                    ct = new CancellationTokenSource();
                    _liveTask = Task.Run(async () =>
                    {

                        //mLiveTaskCompleated = new TaskCompletionSource<bool>();
                        _cancellToken = ct;
                        _activated = true;
                        ChangeNotify(nameof(Activate));
                        await ExecuteAsync(_cancellToken.Token);
                    }, ct.Token);
                    await _liveTask;
                }
                catch (OperationCanceledException)
                {
                    Status = "close connection";
                    Debug.WriteLine($"Cancel liveupdate");
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("WARNING exception "
                    + ex.Message + " "
                    + ex.GetType() + " "
                    + ex.StackTrace + "\n");
                }
                finally
                {
                    switch (_liveTask.Status)
                    {
                        case TaskStatus.RanToCompletion:
                        case TaskStatus.Canceled:
                            _liveTask?.Dispose();
                            break;
                        default: break;
                    }
                    //_liveTask = null;
                    ct?.Cancel();
                    ct?.Dispose();
                    //_cancellToken?.Cancel();
                    //_cancellToken?.Dispose();
                    _cancellToken = null;
                    _activated = false;
                    ChangeNotify(nameof(Activate));
                    ClearStatus();
                    //mLiveTaskCompleated.TrySetResult(true);
                }//finally
            }
        }

        private async Task Deactivate()
        {
            try
            {
                if (null != _cancellToken && !_cancellToken.IsCancellationRequested)
                {
                    _cancellToken.Cancel();
                    //await mLiveTaskCompleated.Task;
                }
                using (await semaphore.UseWaitAsync())
                {
                    await Connection?.Disconnect();
                    if (_activated)
                    {
                        _activated = false;
                        ChangeNotify(nameof(Activate));
                        ClearStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("WARNING exception "
                + ex.Message + " "
                + ex.GetType() + " "
                + ex.StackTrace + "\n");
            }
            finally
            {
            }
        }
        public virtual bool Activate
        {
            get => _activated;
            set
            {
                if (value)
                {
                    if (!_activated)
                    {
                        DebugLog.WriteLine("try activate = true");
                        Task.Run(AsyncActivate);
                    }
                }
                else
                {
                    DebugLog.WriteLine("try activate = false");
                    Task.Run(Deactivate);
                }
            }
        }



        private async Task ExecuteAsync(CancellationToken cancelToken)
        {
            const int rssi_update_period = 2;
            int rssi_update_curr = 0;
            while (true)
            {
                try
                {
                    if (mIsAlive)
                    {
                        if (IsEnableQickInfo && !IsMeasurement)
                        {
                            if (false == await QuickReport(cancelToken))
                            {
                                mIsAlive = false;
                                continue;
                            }
                            if (rssi_update_period > rssi_update_curr++)
                            {
                                rssi_update_curr = 0;
                                await Connection.PhyConnection.UpdateRssi();
                            }
                        }
                        else
                        {
                            await Task.Delay(1000, cancelToken);
                            //await CheckStatus();
                        }
                        await Task.Delay(1000, cancelToken);
                    }
                    else
                    {
                        mIsAlive = await StartAlive(cancelToken);
                        if (!mIsAlive)
                            await Task.Delay(2000, cancelToken);
                    }
                }
                catch (IOTimeoutException)
                {
                    mIsAlive = false;
                    Status = "IOEx_Timeout";
                }
                catch (IOErrPkgException)
                {
                    mIsAlive = false;
                    Status = "IOEx_ErrorResponse";
                }
            }// while (true)
        }// ExecuteAsync(CancellationToken cancelToken)

        protected async Task<bool> StartAlive(CancellationToken ct)
        {
            ClearStatus();
            ct.ThrowIfCancellationRequested();
            if (!await Connection.Connect(ct)
                || !await PostConnectInit(ct))
            {
                Debug.WriteLine("StartAlive FAILED");
                await Connection.Disconnect();
                return false;
            }
            Status = Resource.ConnectedStatus;
            Debug.WriteLine("StartAlive OK");
            return true;
        }


        public abstract Task<bool> PostConnectInit(CancellationToken cancellationToken);
        #endregion





        public string Name => ScannedDeviceInfo.Title;
        public string Type
        {
            get
            {
                if (DeviceIndex.Instance.TryGetName(ScannedDeviceInfo.Device.Kind, out string str))
                    return str;
                return string.Empty;
            }
        }
        public string Firmware { get; protected set; }
        public string Battery { get; protected set; }
        public string Temperature { get; protected set; }
        public string RadioFirmware { get; protected set; }
        string _status;
        public string Status { get => _status; set { _status = value; ChangeNotify(); } }
        public Guid Id => ScannedDeviceInfo.Guid;

        protected readonly List<MemStruct> _Memory = new List<MemStruct>();
        public List<MemStruct> Memory => _Memory;
        protected void ClearStatus()
        {
            //IsAlive = false;
            Temperature = "";
            Battery = "";
            Firmware = "";
            RadioFirmware = "";
            Status = "";
            mIsAlive = false;
        }


    }
}
