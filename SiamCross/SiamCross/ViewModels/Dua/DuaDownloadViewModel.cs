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
    public class DuaDownloadViewModel : BaseMeasurementsDownloaderVM
    {
        ISensor _Sensor;
        DuaMesurementsDownloader _Downloader;

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

            Func<Task> tas = () => Sensor.TaskManager.GetModel()
                .Execute(new TaskUpdateDownloads(_Downloader));
            LoadFromDeviceCommand = new AsyncCommand(tas
                , () => null == Sensor.TaskManager.GetModel().CurrentTask
                , null, false, false);
        }


        private async Task StartDownload()
        {
            return;

            Stopwatch _PerfCounter = new Stopwatch();
            _PerfCounter.Restart();

            if (!uint.TryParse(StartRep, out uint startRep))
                return;
            if (!uint.TryParse(CountRep, out uint countRep))
                return;
            Action<float> StepProgress = (float progress) =>
            {
                //Progress = global_progress_start + progress * global_progress_left;
                //ProgressInfo = $"[{(100.0 * Progress).ToString("N2")}%] " + _Info;
            };
            Action<string> InfoProgress = (string info) =>
            {
                //_Info = info;
                //ProgressInfo = $"[{(100.0 * Progress).ToString("N2")}%] " + info;
            };

            var measurements = await _Downloader.Download(startRep, countRep, StepProgress, InfoProgress);


            ToastService.Instance.LongAlert($"Elapsed {_PerfCounter.ElapsedMilliseconds}");

            //foreach (var m in measurements)
            //    await SensorService.MeasurementHandler(m, false);
        }
    }
}
