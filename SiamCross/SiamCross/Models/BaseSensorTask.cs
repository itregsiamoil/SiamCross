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

        float ProgressRemain;
        int ProgressTime;
        static readonly int RefreshPeriod = 500;
        int TimerRetry => ProgressTime / RefreshPeriod;

        public BaseSensorTask(ISensor sensor, string name)
        {
            Sensor = sensor;
            Connection = sensor.Connection;
            Name = name;
        }

        protected Timer CreateProgressTimer(int ms, float progressStart=0.0f)
        {
            Progress = progressStart;
            ProgressRemain = 1f - progressStart;
            ProgressTime = ms;
            return new Timer(new TimerCallback(OnTimer), null, 0, RefreshPeriod);
        }

        protected void OnTimer(object obj)
        {
            var currenProgress = 1f / TimerRetry;
            Progress += currenProgress* ProgressRemain;
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
            if (await Sensor.DoActivate(_Cts.Token))
                return true;
            InfoEx = "не удалось подключиться к прибору";
            return false;
        }
    }
}
