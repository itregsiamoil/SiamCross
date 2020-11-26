#define DEBUG_UNIT
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dynamographs.Shared
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
        public bool IsAlive { get; protected set; }

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
        public abstract Task<bool> QuickReport();
        public abstract Task StartMeasurement(object measurementParameters);

        #endregion
        #region Activate implementation
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private CancellationTokenSource _cancellToken= null;
        private bool _activated = false;
        #endregion
        async void AsyncActivate()
        {
            Task<bool> _liveTask = null; ;
            try
            {
                _cancellToken = new CancellationTokenSource();
                _liveTask = Task.Run(async () =>
                {
                    await ExecuteAsync(_cancellToken.Token);
                    return false;
                }, _cancellToken.Token);
                _activated = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
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
                Deactivate();
                _liveTask?.Dispose();//_liveTask = null;
                
            }
        }

        void Deactivate()
        {
            try
            {
                mConnection?.Disconnect();
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
                _cancellToken?.Cancel();
                _cancellToken?.Dispose();
                _cancellToken = null;
                _activated = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Activate"));
                ClearStatus();
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
                    //using (await semaphore.UseWaitAsync())
                    {
                        if (!_activated )
                        {
                            DebugLog.WriteLine("try activate = true");
                            AsyncActivate();
                        }
                    }
                }
                else
                {
                    //using (await semaphore.UseWaitAsync())
                    {
                        DebugLog.WriteLine("try activate = false");
                        if (null != _cancellToken && !_cancellToken.IsCancellationRequested)
                            _cancellToken.Cancel();
                        else
                            Deactivate();
                    }
                }
            }
        }
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await Task.Delay(1000);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (IsAlive)
                    {
                        if (false == IsMeasurement)
                        {
                            if (false == await QuickReport())
                            {
                                IsAlive = (ConnectionState.Connected == mConnection.State);
                                continue;
                            }
                                
                        }
                        else
                        {
                            await Task.Delay(1000, cancellationToken);
                            //await CheckStatus();
                        }
                        await Task.Delay(1000, cancellationToken);
                    }
                    else
                    {
                        ClearStatus();
                        cancellationToken.ThrowIfCancellationRequested();
                        bool connected = await mConnection.Connect();
                        if (!connected)
                            await Task.Delay(2000, cancellationToken);
                        else
                        {
                            if (await PostConnectInit())
                            {
                                IsAlive = true;
                                SensorData.Status = Resource.ConnectedStatus;
                            }

                        }

                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                DebugLog.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("WARNING exception in"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
        }
        public abstract Task<bool> PostConnectInit();
        #endregion

        protected void ClearStatus()
        {
            //IsAlive = false;
            SensorData.Temperature = "";
            SensorData.Battery = "";
            SensorData.Firmware = "";
            SensorData.RadioFirmware = "";
            SensorData.Status = "";
        }
        

    }
}
