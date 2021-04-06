using SiamCross.Models.Connection.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskUpdateDownloads : BaseTask
    {
        DuaMesurementsDownloader _Downloader;
        public TaskUpdateDownloads(object obj)
        {
            if (obj is DuaMesurementsDownloader model)
                _Downloader = model;
        }

        static int ProgressTime = 25000;
        static int RefreshPriod = 500;
        static int TimerRetry => ProgressTime / RefreshPriod;

        public override async Task<bool> DoExecute()
        {
            if (null == _Downloader || null==Manager)
                return false;

            bool ret = false;
            Progress = 0.01f;
            using (var timer = new Timer(new TimerCallback(Count), null, 0, RefreshPriod))
            {
                ret = RespResult.NormalPkg == await _Downloader.Update(_Cts.Token, Manager.Info);
            }
            Progress = 1f;
            return ret;
        }
        void Count(object obj)
        {
            Progress += 1f / TimerRetry;
        }
    }
}
