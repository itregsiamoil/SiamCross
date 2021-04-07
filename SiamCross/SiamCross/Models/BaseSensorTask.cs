using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public abstract class BaseSensorTask : BaseTask
    {
        public ISensor Sensor;
        public IProtocolConnection Connection;

        uint ProgressTime;
        static readonly uint RefreshPeriod = 500;
        uint TimerRetry => ProgressTime / RefreshPeriod;

        public BaseSensorTask(ISensor sensor, string name)
        {
            Sensor = sensor;
            Connection = sensor.Connection;
            Name = name;
        }

        protected Timer CreateProgressTimer(uint ms)
        {
            ProgressTime = ms;
            return new Timer(new TimerCallback(OnTimer), null, 0, RefreshPeriod);
        }

        protected void OnTimer(object obj)
        {
            Progress += 1f / TimerRetry;
        }

        public async Task<bool> RetryExecAsync(uint retry, Func<Task<bool>> fn)
        {
            bool isOk = false;
            for (int i = 0; i < retry
                && !_Cts.IsCancellationRequested
                && !isOk; ++i)
            {
                isOk = await fn.Invoke();
            }
            return isOk;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            InfoEx = Resource.StatConn_PendingConnect;
            if (!await Sensor.DoActivate(_Cts.Token))
            {
                InfoEx = "не удалось подключиться к прибору";
                return false;
            }
            return true;
        }
    }
}
