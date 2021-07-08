using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public abstract class BaseSensorTask : BaseTask
    {
        public SensorModel Sensor;
        public IProtocolConnection Connection => Sensor.Connection;

        float ProgressRemain;
        TimeSpan ProgressTime;
        static readonly int RefreshPeriod = 500;
        uint TimerRetry => (uint)(ProgressTime.TotalMilliseconds / RefreshPeriod + 1);

        public BaseSensorTask(SensorModel sensor, string name)
        {
            Sensor = sensor;
            Name = name;
        }

        protected Timer CreateProgressTimer(TimeSpan t, float progressStart = 0.0f)
        {
            Progress = progressStart;
            ProgressRemain = 1f - progressStart;
            ProgressTime = t;
            return new Timer(new TimerCallback(OnTimer), null, 0, RefreshPeriod);
        }
        protected Timer CreateProgressTimer(int ms, float progressStart = 0.0f)
        {
            return CreateProgressTimer(TimeSpan.FromMilliseconds(ms), progressStart);
        }

        protected void OnTimer(object obj)
        {
            var currenProgress = 1f / TimerRetry;
            Progress += currenProgress * ProgressRemain;
        }

        public async Task<bool> RetryExecAsync(uint retry, Func<CancellationToken, Task<bool>> fn, CancellationToken ct)
        {
            bool isOk = false;
            for (int i = 0; i < retry
                && !ct.IsCancellationRequested
                && !isOk; ++i)
            {
                isOk = await fn.Invoke(ct);
            }
            return isOk;
        }

        public async Task<bool> CheckConnectionAsync(CancellationToken ct)
        {
            if (Models.Connection.ConnectionState.Connected == Connection.State)
                return true;
            InfoEx = Resource.StatConn_PendingConnect;

            bool connected = false;
            for (int i = 0; i < Connection.Retry
                            && !connected
                            && !ct.IsCancellationRequested; ++i)
                connected = await Connection.Connect(ct);
            if (connected && !Sensor.ConnHolder.IsActivated)
                Sensor.ConnHolder.IsActivated = true;
            if (!connected)
                InfoEx = Resource.Failed_connect;
            return connected;
        }
    }
}
