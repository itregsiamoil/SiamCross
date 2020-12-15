﻿#define DEBUG_UNIT
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    abstract public class BaseSensor : ISensor, INotifyPropertyChanged
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
                if("State" == a.PropertyName)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnStateStr"));
            };
            //mConnection.PropertyChanged += PropertyChanged;
        }
        public void Dispose()
        {
            _cancellToken?.Cancel();
            mConnection?.Disconnect();
        }
        #endregion
        #region basic implementation
        #region Variables
        private IProtocolConnection mConnection;
        #endregion
        public string ConnStateStr => ConnectionStateAdapter.ToString(Connection.State);
        public int MeasureProgressP
        {
            get => (int)(mMeasureProgress*100);
        }

        float mMeasureProgress = 0;
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
        bool mIsAlive = false;
        public bool IsAlive 
        {
            get => mIsAlive;
            protected set
            {
                mIsAlive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAlive"));
            }
        }

        bool mIsMeasurement = false;
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
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private CancellationTokenSource _cancellToken= null;
        //TaskCompletionSource<bool> mLiveTaskCompleated=null;
        Task<bool> _liveTask = null;
        private bool _activated = false;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        #endregion
        async void AsyncActivate()
        {
            try
            {
                CancellationTokenSource ct = new CancellationTokenSource();
                _liveTask = Task.Run(async () =>
                {
                    using (await semaphore.UseWaitAsync())
                    {
                        //mLiveTaskCompleated = new TaskCompletionSource<bool>();
                        _cancellToken = ct;
                        _activated = true;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
                        await ExecuteAsync(_cancellToken.Token);
                    }
                    return false;
                }, ct.Token);
                _activated = await _liveTask;
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
                using (await semaphore.UseWaitAsync())
                {
                    switch(_liveTask.Status)
                    {
                        case TaskStatus.RanToCompletion:
                        case TaskStatus.Canceled:
                            _liveTask?.Dispose();
                            break;
                        default:break;
                    }
                    //_liveTask = null;
                    _cancellToken?.Cancel();
                    _cancellToken?.Dispose();
                    _cancellToken = null;
                    _activated = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
                    ClearStatus();
                    //mLiveTaskCompleated.TrySetResult(true);
                }
            }//finally
        }

        async void Deactivate()
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
                    mConnection?.Disconnect();
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
                DebugLog.WriteLine("try activate");
                if (value)
                {
                    if (!_activated )
                    {
                        DebugLog.WriteLine("try activate = true");
                        AsyncActivate();
                    }
                }
                else
                {
                //using (await semaphore.UseWaitAsync())
                    DebugLog.WriteLine("try activate = false");
                    Deactivate();
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
                                IsAlive = (ConnectionState.Connected == mConnection.State);
                                await mConnection.Disconnect();
                                continue;
                            }
                            if (rssi_update_period > rssi_update_curr++)
                            {
                                rssi_update_curr = 0;
                                Connection.UpdateRssi();
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
                catch (IOEx_Timeout)
                {
                    IsAlive = false;
                    SensorData.Status = "IOEx_Timeout";
                }
                catch (IOEx_ErrorResponse)
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
            bool connected = await mConnection.Connect();
            if (!connected)
                return false;
            if (!await PostConnectInit(cancelToken))
                return false;
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
        public string GetStringPayload(byte[] pkg)
        {
            Span<byte> payload = pkg.AsSpan(12, pkg.Length - 12 - 2);
            if (payload.Length > 20)
                return Encoding.UTF8.GetString(payload.ToArray());
            return Encoding.GetEncoding(1251).GetString(payload.ToArray());
        }


    }
}
