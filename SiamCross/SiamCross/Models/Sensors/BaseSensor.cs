﻿#define DEBUG_UNIT
using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.ViewModels.MeasurementViewModels;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSensor : ISensor
    {
        #region TmpVariables
        #endregion
        #region Constructors & Destructors
        //TaskScheduler _uiScheduler;
        protected BaseSensor(IProtocolConnection conn, ScannedDeviceInfo dev_info)
        {
            ScannedDeviceInfo = dev_info;
            Firmware = "";
            Battery = "";
            Temperature = "";
            RadioFirmware = "";
            Status = "";

            mConnection = conn;
            IsAlive = false;
            IsMeasurement = false;
            // Получение планировщика UI для потока, который создал форму:
            //_uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Connection.PropertyChanged += (obj, a) =>
            {
                if (null == a || "State" != a.PropertyName)
                    return;
                ChangeNotify(nameof(ConnStateStr));
            };
            //mConnection.PropertyChanged += PropertyChanged;
            ShowDetailViewCommand = new AsyncCommand(
                () => App.NavigationPage.Navigation.PushAsync(new SensorDetailsView(this))
                , (Func<object, bool>)null, null, false, false);
            
            ShowSurveysViewCommand = new AsyncCommand(
                ShowSurveysCollection
            , (Func<object, bool>)null, null, false, false);

        }
        private async Task ShowSurveysCollection()
        {
            var type = typeof(SurveysCollectionnViewModel);


            var view = ViewFactoryService.Get(type) as SurveysCollectionnView;
            if (null == view)
            {
                view = new SurveysCollectionnView();
                ViewFactoryService.Register(type, view);
            }

            var surveys = new ObservableCollection<SurveyVM>();
            Surveys.ForEach(o => surveys.Add(o));
            var ctx = new SurveysCollectionnViewModel()
            {
                Sensor = this,
                SurveysCollection = surveys
            };
            view = ViewFactoryService.Get<SurveysCollectionnView>(type, ctx);


            await App.NavigationPage.Navigation.PushAsync(view);
        }

        public async void Dispose()
        {
            await Deactivate();
            await mConnection.Disconnect();
        }
        #endregion
        #region basic implementation
        #region Variables
        private readonly IProtocolConnection mConnection;
        #endregion
        public string ConnStateStr => ConnectionStateAdapter.ToString(Connection.State);
        public int MeasureProgressP => (int)(mMeasureProgress * 100);

        private float mMeasureProgress = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public void ChangeNotify([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
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
        public IProtocolConnection Connection => mConnection;

        private bool mIsAlive = false;
        public bool IsAlive
        {
            get => mIsAlive;
            protected set
            {
                mIsAlive = value;
                ChangeNotify();
            }
        }

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
        private bool _IsEnableQickInfo = true;
        public bool IsEnableQickInfo
        {
            get => _IsEnableQickInfo;
            set
            {
                _IsEnableQickInfo = value;
                ChangeNotify();
            }
        }
        PositionInfoVM _Position = new PositionInfoVM();
        public PositionInfoVM Position
        {
            get => _Position;
            set => _Position = value;
        }
        public ICommand ShowDetailViewCommand { get; set; }
        public ICommand ShowFactoryConfigViewCommand { get; set; }
        public ICommand ShowUserConfigViewCommand { get; set; }

        public ICommand ShowSurveysViewCommand { get; set; }

        public ICommand ShowStateViewCommand { get; set; }
        public ICommand ShowDownloadsViewCommand { get; set; }

        public IReadOnlyCollection<SurveyVM> Surveys { get; set; }

        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }
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
                    if (null != mConnection)
                    {
                        if (ConnectionState.Disconnected != mConnection.State)
                            await mConnection?.Disconnect();
                    }
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
        public bool Activate
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

        private async Task ExecuteAsync(CancellationToken cancelToken)
        {
            const int rssi_update_period = 2;
            int rssi_update_curr = 0;
            while (true)
            {
                try
                {
                    if (IsAlive)
                    {
                        if (IsEnableQickInfo && !IsMeasurement)
                        {
                            if (false == await QuickReport(cancelToken))
                            {
                                IsAlive = false;
                                continue;
                            }
                            if (rssi_update_period > rssi_update_curr++)
                            {
                                rssi_update_curr = 0;
                                Connection.PhyConnection.UpdateRssi();
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
                        IsAlive = await StartAlive(cancelToken);
                        if (!IsAlive)
                            await Task.Delay(2000, cancelToken);
                    }
                }
                catch (IOTimeoutException)
                {
                    IsAlive = false;
                    Status = "IOEx_Timeout";
                }
                catch (IOErrPkgException)
                {
                    IsAlive = false;
                    Status = "IOEx_ErrorResponse";
                }
            }// while (true)
        }// ExecuteAsync(CancellationToken cancelToken)

        private async Task<bool> StartAlive(CancellationToken cancelToken)
        {
            ClearStatus();
            cancelToken.ThrowIfCancellationRequested();
            if (!await mConnection.Disconnect()
                || !await mConnection.Connect()
                || !await PostConnectInit(cancelToken))
            {
                return false;
            }
            Status = Resource.ConnectedStatus;
            return true;
        }


        public abstract Task<bool> PostConnectInit(CancellationToken cancellationToken);
        #endregion

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
            IsAlive = false;
        }


    }
}
