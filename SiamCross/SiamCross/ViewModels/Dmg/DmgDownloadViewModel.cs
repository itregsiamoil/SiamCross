using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Services;
using SiamCross.Services.Toast;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.Dmg
{
    public class DmgDownloadViewModel : BaseVM
    {
        ISensor _Sensor;
        DmgMesurementsDownloader _Downloader;
        bool _IsDownloading;
        float _Progress;
        string _ProgressInfo;
        string _Aviable;

        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;
                _Downloader = _Sensor.Downloader as DmgMesurementsDownloader;
                ChangeNotify();
            }
        }
        public bool IsDownloading
        {
            get => _IsDownloading;
            set { _IsDownloading = value; ChangeNotify(); }
        }
        public float Progress
        {
            get => _Progress;
            set { _Progress = value; ChangeNotify(); }
        }
        string _Info;
        public string ProgressInfo
        {
            get => _ProgressInfo;
            set { _ProgressInfo = value; ChangeNotify(); }
        }
        public string Aviable
        {
            get => _Aviable;
            set { _Aviable = value; ChangeNotify(); }
        }

        public bool OpenOnDownload { get; set; }



        public ICommand LoadFromDeviceCommand { get; set; }

        public ICommand StartDownloadCommand { get; set; }

        public DmgDownloadViewModel(ISensor sensor)
        {
            Sensor = sensor;

            StartDownloadCommand = new AsyncCommand(StartDownload
                , (Func<object, bool>)null, null, false, false);

            LoadFromDeviceCommand = new AsyncCommand(LoadFromDevice
                , (Func<object, bool>)null, null, false, false);

        }

        private async Task LoadFromDevice()
        {
            ProgressInfo = "получение информации с прибора";
            IsDownloading = true;
            Progress = 0.1f;
            if (RespResult.NormalPkg != await _Sensor.Downloader.Update())
            {
                Progress = 1.0f;
                ProgressInfo = "не удалось получить информация с прибора";
                return;
            }
            Progress = 0.8f;
            Aviable = _Downloader.Aviable().ToString();
            Progress = 0.9f;
            IsDownloading = false;
            Progress = 1.0f;
        }

        private async Task StartDownload()
        {
            Stopwatch _PerfCounter = new Stopwatch();
            _PerfCounter.Restart();

            ProgressInfo = "Считывание с прибора";
            IsDownloading = true;
            float global_progress_start = 0.00f;
            float global_progress_left = 1.0f;
            Action<float> StepProgress = (float progress) =>
            {
                Progress = global_progress_start + progress * global_progress_left;
                ProgressInfo = $"[{(100.0 * Progress).ToString("N2")}%] " + _Info;
            };
            Action<string> InfoProgress = (string info) =>
            {
                _Info = info;
                ProgressInfo = $"[{(100.0 * Progress).ToString("N2")}%] " + info;
            };

            var measurements = await _Downloader.Download(1, 1, StepProgress, InfoProgress);

            IsDownloading = false;
            ToastService.Instance.LongAlert($"Elapsed {_PerfCounter.ElapsedMilliseconds}");
            await SensorService.MeasurementHandler(measurements[0], OpenOnDownload);


        }
    }
}
