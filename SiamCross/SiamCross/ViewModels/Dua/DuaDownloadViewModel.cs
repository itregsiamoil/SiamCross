using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dua;
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
        string _Info;
        string _ProgressInfo;
        string _AviableRep;
        string _AviableEcho;
        string _StartRep;
        string _CountRep;
        string _StartEcho;
        string _CountEcho;

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
        public string ProgressInfo
        {
            get => _ProgressInfo;
            set { _ProgressInfo = value; ChangeNotify(); }
        }
        public string AviableRep
        {
            get => _AviableRep;
            set { _AviableRep = value; ChangeNotify(); }
        }
        public string AviableEcho
        {
            get => _AviableEcho;
            set { _AviableEcho = value; ChangeNotify(); }
        }
        public string StartRep
        {
            get => _StartRep;
            set { _StartRep = value; ChangeNotify(); }
        }
        public string CountRep
        {
            get => _CountRep;
            set { _CountRep = value; ChangeNotify(); }
        }
        public string StartEcho
        {
            get => _StartEcho;
            set { _StartEcho = value; ChangeNotify(); }
        }
        public string CountEcho
        {
            get => _CountEcho;
            set { _CountEcho = value; ChangeNotify(); }
        }

        public ICommand LoadFromDeviceCommand { get; set; }
        public ICommand StartDownloadCommand { get; set; }

        public DuaDownloadViewModel(ISensor sensor)
            : this()
        {
            Sensor = sensor;
        }
        public DuaDownloadViewModel()
        {
            StartRep = "0";
            CountRep = "1";
            StartEcho = "0";
            CountEcho = "1";

            StartDownloadCommand = new AsyncCommand(StartDownload
                , (Func<object, bool>)null, null, false, false);

            LoadFromDeviceCommand = new AsyncCommand(LoadFromDevice
                , (Func<object, bool>)null, null, false, false);

        }

        private async Task LoadFromDevice()
        {
            IsDownloading = true;
            ProgressInfo = "получение информации с прибора";
            Progress = 0.1f;

            if (RespResult.NormalPkg != await _Downloader.Update())
            {
                Progress = 1.0f;
                ProgressInfo = "не удалось получить информация с прибора";
                return;
            }
            Progress = 0.5f;
            AviableRep = _Downloader.AviableRep().ToString();
            AviableEcho = _Downloader.AviableEcho().ToString();
            Progress = 1.0f;
            IsDownloading = false;
        }

        private async Task StartDownload()
        {
            Stopwatch _PerfCounter = new Stopwatch();
            _PerfCounter.Restart();

            if (!uint.TryParse(StartRep, out uint startRep))
                return;
            if (!uint.TryParse(CountRep, out uint countRep))
                return;

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

            var measurements = await _Downloader.Download(startRep, countRep, StepProgress, InfoProgress);

            IsDownloading = false;
            ToastService.Instance.LongAlert($"Elapsed {_PerfCounter.ElapsedMilliseconds}");

            //foreach (var m in measurements)
            //    await SensorService.MeasurementHandler(m, false);
        }
    }
}
