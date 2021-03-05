﻿#define DEBUG_UNIT
using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSensor : ISensor
    {
        #region TmpVariables
        #endregion
        #region Constructors & Destructors
        //TaskScheduler _uiScheduler;
        protected BaseSensor(IProtocolConnection conn, SensorData sensorData)
        {
            mConnection = conn;
            IsAlive = false;
            SensorData = sensorData;
            IsMeasurement = false;
            // Получение планировщика UI для потока, который создал форму:
            //_uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Connection.PropertyChanged += (obj, a) =>
            {
                if (null == a || "State" != a.PropertyName)
                    return;
                OnPropChange(new PropertyChangedEventArgs("ConnStateStr"));
            };
            //mConnection.PropertyChanged += PropertyChanged;
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
        public void OnPropChange(PropertyChangedEventArgs arg)
        {
            PropertyChanged?.Invoke(this, arg);
        }
        public float MeasureProgress
        {
            get => mMeasureProgress;
            set
            {
                mMeasureProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MeasureProgress"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MeasureProgressP"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAlive"));
            }
        }

        private bool mIsMeasurement = false;
        public bool IsMeasurement
        {
            get => mIsMeasurement;
            protected set
            {
                mIsMeasurement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMeasurement"));
            }
        }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }
        public abstract Task<bool> QuickReport(CancellationToken cancellationToken);
        //public virtual Task<bool> UpdateRssi(CancellationToken cancellationToken);
        public abstract Task StartMeasurement(object measurementParameters);

        #endregion
        #region Activate implementation
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged;
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
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
                        await ExecuteAsync(_cancellToken.Token);
                    }, ct.Token);
                    await _liveTask;
                }
                catch (OperationCanceledException)
                {
                    SensorData.Status = "close connection";
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
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
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
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
                        if (false == IsMeasurement)
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
                    SensorData.Status = "IOEx_Timeout";
                }
                catch (IOErrPkgException)
                {
                    IsAlive = false;
                    SensorData.Status = "IOEx_ErrorResponse";
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
            SensorData.Status = Resource.ConnectedStatus;
            return true;
        }


        public abstract Task<bool> PostConnectInit(CancellationToken cancellationToken);
        #endregion

        protected void ClearStatus()
        {
            //IsAlive = false;
            SensorData.Temperature = "";
            SensorData.Battery = "";
            SensorData.Firmware = "";
            SensorData.RadioFirmware = "";
            SensorData.Status = "";
            IsAlive = false;
        }


    }
}
