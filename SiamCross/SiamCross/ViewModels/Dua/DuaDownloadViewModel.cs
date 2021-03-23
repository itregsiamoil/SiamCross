using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dua;
using SiamCross.Services;
using SiamCross.Services.Toast;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.Dua
{
    public class DuaDownloadViewModel : BaseVM
    {
        ISensor _Sensor;
        DuaMesurementsDownloader _Downloader;

        bool _IsDownloading;
        float _Progress;
        string _ProgressInfo;
        string _AviableRep;
        string _AviableEcho;

        public ISensor Sensor
        {
            get => _Sensor;
            set 
            { 
                _Sensor = value;
                _Downloader = _Sensor.Downloader as DuaMesurementsDownloader;
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
            get => _AviableRep;
            set { _AviableRep = value; ChangeNotify(); }
        }
        public string AviableEcho
        {
            get => _AviableEcho;
            set { _AviableEcho = value; ChangeNotify(); }
        }



        public bool OpenOnDownload { get; set; }



        public ICommand LoadFromDeviceCommand { get; set; }

        public ICommand StartDownloadCommand { get; set; }

        public DuaDownloadViewModel()
        {
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
            await _Sensor.Downloader.Update();
            Progress = 0.5f;
            Aviable = _Downloader.Aviable().ToString();
            AviableEcho = _Downloader.AviableEcho().ToString();
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

            var measurement = await _Sensor.Downloader.Download(1, 1, StepProgress, InfoProgress);

            IsDownloading = false;
            ToastService.Instance.LongAlert($"Elapsed {_PerfCounter.ElapsedMilliseconds}");
            await SensorService.MeasurementHandler(measurement, OpenOnDownload);


        }
    }
}
