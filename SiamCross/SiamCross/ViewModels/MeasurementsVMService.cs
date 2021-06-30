using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Email;
using SiamCross.Services.Environment;
using SiamCross.Services.Logging;
using SiamCross.Services.MediaScanner;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MeasurementsVMService : BasePageVM
    {
        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }
        private static readonly Lazy<MeasurementsVMService> _instance =
            new Lazy<MeasurementsVMService>(() => new MeasurementsVMService());
        public static MeasurementsVMService Instance => _instance.Value;


        private string _title;
        private List<Ddin2Measurement> _ddin2Measurements;
        private List<DuMeasurement> _duMeasurements;
        //private List<MeasureData> _Measurements = new List<MeasureData>();


        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private bool _selectMode = false;

        public bool SelectMode
        {
            get => _selectMode;
            set
            {
                _selectMode = value;
                ChangeNotify(nameof(SelectMode));
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                ChangeNotify(nameof(Title));
                ChangeNotify(nameof(SelCount));
            }
        }
        public int SelCount => SelectedMeasurements.Count;

        public ObservableRangeCollection<MeasurementView> Measurements { get; }
        public ObservableRangeCollection<object> SelectedMeasurements { get; }

        public ICommand RefreshCommand { get; set; }
        public ICommand SelectAllCommand { get; set; }
        public ICommand UnselectAllCommand { get; set; }

        public ICommand ShareCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ICommand OnItemLongPressCommand { get; set; }
        public ICommand OnItemTappedCommand { get; set; }

        private MeasurementsVMService()
        {
            Measurements = new ObservableRangeCollection<MeasurementView>();
            SelectedMeasurements = new ObservableRangeCollection<object>();
            Title = Resource.MeasurementsTitle;
            RefreshCommand = new AsyncCommand(ReloadMeasurementsFromDb);
            SelectAllCommand = new Command(SelectAll);
            UnselectAllCommand = new Command(UnselectAll);
            ShareCommand = new AsyncCommand(ShareMeasurementsAsync);
            SendCommand = new AsyncCommand(SendMeasurementsAsync);
            SaveCommand = new AsyncCommand(SaveMeasurementsAsync);
            DeleteCommand = new AsyncCommand(DeleteMeasurementsAsync);

            OnItemTappedCommand = new Command(obj => OnItemTapped(obj as MeasurementView));
            OnItemLongPressCommand = new Command(obj => OnItemLongPress(obj as MeasurementView));

        }
        public void DoOnDisappearing()
        {
            SelectedMeasurements.Clear();
            Measurements?.Clear();
            _ddin2Measurements?.Clear();
            _duMeasurements?.Clear();
        }
        public async Task ReloadMeasurementsFromDb()
        {
            try
            {
                Stopwatch perf = new Stopwatch();
                perf.Restart();

                DoOnDisappearing();
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Clear all");

                var meas = new List<MeasurementView>();
                _ddin2Measurements = DbService.Instance.GetDdin2Measurements().ToList();
                _duMeasurements = DbService.Instance.GetDuMeasurements().ToList();
                var measurements = (await DbService.Instance.GetSurveysAsync());
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Load from db");

                foreach (var m in measurements)
                {
                    meas.Add(new MeasurementView(m.MeasureData));
                }
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Set DUA");


                foreach (Ddin2Measurement m in _ddin2Measurements)
                {
                    var measView = new MeasurementView(Convert(m));
                    meas.Add(measView);
                }
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Set DDIN");

                foreach (DuMeasurement m in _duMeasurements)
                {
                    var measView = new MeasurementView(Convert(m));
                    meas.Add(measView);
                }
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Set DU");

                Measurements.ReplaceRange(meas.OrderByDescending(m => m.BeginTimestamp));
                Debug.WriteLine($"[{perf.ElapsedMilliseconds}]Sort and set to gui");
                perf.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetMeasurementFromDb method" + "\n");
            }
        }
        public bool OnBackButton()
        {
            if (SelectMode)
            {
                UnselectAll();
                SelectMode = false;
                Title = Resource.MeasurementsTitle;
                return true;
            }
            return false;
        }
        public void UpdateSelectTitle()
        {
            Title = $"{Resource.SelectedMeasurements}: {SelCount}";
        }
        public void UpdateSelect(MeasurementView item, bool select)
        {
            if (select)
            {
                if (!SelectedMeasurements.Contains(item))
                    SelectedMeasurements.Add(item);
            }
            else
            {
                SelectedMeasurements.Remove(item);
            }
        }
        public async void OnItemTapped(MeasurementView item)
        {
            if (SelectMode)
            {
                if (null == item)
                    return;
                // при срабатывании события INotifyPropertyChanged.PropertyChanged
                // произойдёт вызов UpdateSelect
                item.IsSelected = !item.IsSelected;
            }
            else
                await PushPageAsync(item);
        }
        private void OnItemLongPress(MeasurementView item)
        {
            SelectMode = true;
            Title = $"{Resource.SelectedMeasurements}: {SelCount}";
            //item.IsSelected = true;
        }
        public void UpdateSelectedItems(IReadOnlyList<object> prev, IReadOnlyList<object> curr)
        {
            Title = $"{Resource.SelectedMeasurements}: {SelectedMeasurements.Count}";
            IEnumerable<object> unselected = prev.Except(curr);
            foreach (MeasurementView item in unselected)
                item.IsSelected = false;

            IEnumerable<MeasurementView> sel = curr.Cast<MeasurementView>();
            IEnumerable<MeasurementView> selected = sel.Where(element => !element.IsSelected);
            foreach (MeasurementView item in selected)
                item.IsSelected = true;
        }
        private void UnselectAll()
        {
            if (!SelectMode)
                return;

            Stopwatch perf = new Stopwatch();
            perf.Restart();
            SelectedMeasurements.ReplaceRange(new List<MeasurementView>());
            IEnumerable<MeasurementView> selected = Measurements.Where(element => element.IsSelected);
            foreach (MeasurementView item in selected)
            {
                item.IsSelected = false;
                //UpdateSelect(item, item.IsSelected);
            }
            Debug.WriteLine($"[{perf.ElapsedMilliseconds}] OLD UnselectAll");
            UpdateSelectTitle();
        }
        private void SelectAll()
        {
            if (!SelectMode)
                SelectMode = true;
            Stopwatch perf = new Stopwatch();
            perf.Restart();
            SelectedMeasurements.ReplaceRange(new List<MeasurementView>(Measurements));
            IEnumerable<MeasurementView> selected = Measurements.Where(element => !element.IsSelected);
            foreach (MeasurementView item in selected)
            {
                item.IsSelected = true;
                //UpdateSelect(item, item.IsSelected);
            }
            Debug.WriteLine($"[{perf.ElapsedMilliseconds}] OLD SelectAll");
            UpdateSelectTitle();
        }
        private async Task ShareMeasurementsAsync()
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                var paths = await SaveXmlsReturnPaths(SelectedMeasurements);

                List<ShareFile> sf = new List<ShareFile>();
                foreach (var p in paths)
                {
                    sf.Add(new ShareFile(p));
                }
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = $"{Resource.SelectedMeasurements}: {paths.Count}",
                    Files = sf
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler" + "\n");
                await Application.Current.MainPage.DisplayAlert(Resource.Error,
                    ex.Message, "OK");
            }
        }
        private async Task SendMeasurementsAsync()
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                if (!ValidateForEmptiness())
                    return;

                foreach (MeasurementView survay in SelectedMeasurements)
                {
                    survay.IsRunning = true;
                    survay.LastSentRecipient = Settings.Instance.ToAddress;
                }

                //ToastService.Instance.LongAlert($"{Resource.SendingMeasurements}...");
                var paths = await SaveXmlsReturnPaths(SelectedMeasurements);



                bool is_ok = await EmailService.Instance.SendEmailWithFilesAsync(Settings.Instance.SubjectName,
                    "SiamCompany Telemetry Transfer Service",
                    paths);
                //ToastService.Instance.LongAlert($"{SelectedMeasurements.Count} {Resource.MeasurementsSentSuccesfully}");

                foreach (MeasurementView sent_sur in SelectedMeasurements)
                {
                    sent_sur.LastSentTimestamp = DateTime.Now.ToString();
                    sent_sur.LastSentRecipient = Settings.Instance.ToAddress;
                    sent_sur.IsRunning = false;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SendMeasurements command handler" + "\n");
                await Application.Current.MainPage.DisplayAlert(Resource.Error,
                    ex.Message, "OK");
                foreach (MeasurementView err_sur in SelectedMeasurements)
                {
                    err_sur.LastSentTimestamp = "";
                    err_sur.LastSentRecipient = "";
                    err_sur.IsRunning = false;
                }
            }
        }
        private async Task SaveMeasurementsAsync()
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                //ToastService.Instance.LongAlert($"{Resource.SavingMeasurements}...");
                foreach (MeasurementView survay in SelectedMeasurements)
                    survay.IsRunning = true;

                var paths = await SaveXmlsReturnPaths(SelectedMeasurements);

                string ts = DateTime.Now.ToString();
                string dir = EnvironmentService.Instance.GetDir_Measurements();
                foreach (MeasurementView m in SelectedMeasurements)
                {
                    m.IsRunning = false;
                    m.LastSaveTimestamp = ts;
                    m.LastSaveFolder = dir;
                }
                //string savePath = @"""Measurements""";
                //ToastService.Instance.LongAlert($"{SelectedMeasurements.Count} " +
                //    $"{Resource.MeasurementsSavedSuccesfully} {savePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveMeasurements method" + "\n");
                await Application.Current.MainPage.DisplayAlert(Resource.Error,
                ex.Message, "OK");
                foreach (MeasurementView m in SelectedMeasurements)
                    m.IsRunning = false;
            }
        }
        private async Task DeleteMeasurementsAsync()
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;

                bool FileResult = await Application.Current.MainPage
                    .DisplayAlert("", Resource.DeleteQuestion, Resource.Ok, Resource.Cancel);

                if (!FileResult)
                    return;

                if (SelectedMeasurements.Count != 0)
                {
                    foreach (object m in SelectedMeasurements)
                    {
                        if (m is MeasurementView mv)
                        {
                            Measurements.Remove(mv);
                            switch (mv.MeasureKind)
                            {
                                case 0: DbService.Instance.RemoveDdin2Measurement(mv.Id); break;
                                case 1:
                                    if (0x1201 == mv.MeasureData.Device.Kind)
                                        await DbService.Instance.DelSurveyAsync(mv.Id);
                                    else
                                        await DbService.Instance.RemoveDuMeasurementAsync(mv.Id);
                                    break;
                                default:
                                    await DbService.Instance.DelSurveyAsync(mv.Id);
                                    break;
                            }
                        }
                    }
                    MessagingCenter.Send(this, "RefreshAfterDeleting");
                    SelectedMeasurements.Clear();
                    Title = $"{Resource.SelectedMeasurements}: {SelCount}";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteMeasurements method" + "\n");
            }
        }
        public async Task PushPageAsync(MeasurementView survey)
        {
            if (null == survey)
                return;
            UnselectAll();

            try
            {
                switch (survey.MeasureKind) // MeasurementIndex.Instance.
                {
                    default: break;
                    case 0:
                        Ddin2Measurement ddin_meas = _ddin2Measurements?
                            .SingleOrDefault(m => m.Id == survey.Id);
                        if (ddin_meas != null)
                            if (CanOpenPage(typeof(Ddin2MeasurementDonePage)))
                            {
                                await App.NavigationPage.Navigation
                                    .PushAsync(
                                        new Ddin2MeasurementDonePage(ddin_meas), true);
                            }
                        break;
                    case 1:
                        DuMeasurement du = null;
                        if (0x1201 == survey.MeasureData.Device.Kind)
                        {
                            await DbService.Instance.GetValuesAsync(survey.MeasureData);
                            du = await ConvertToOldMeasurementAsync(survey.MeasureData);
                        }
                        else
                        {
                            du = _duMeasurements?.SingleOrDefault(m => m.Id == survey.Id);
                        }

                        if (du != null && CanOpenPage(typeof(DuMeasurementDonePage)))
                        {
                            await App.NavigationPage.Navigation
                                .PushAsync(
                                    new DuMeasurementDonePage(du), true);
                        }
                        break;
                    case 2:
                        if (0x1700 == survey.MeasureData.Device.Kind)
                        {
                            await DbService.Instance.GetValuesAsync(survey.MeasureData);
                            var model = new SurveyDoneModel(survey.MeasureData);
                            var vm = new SurveyDoneVM(model);

                            var page = new Views.Umt.SurveyDonePage();
                            page.BindingContext = vm;

                            await App.NavigationPage.Navigation.PushAsync(page);

                        }

                        break;

                }
            }
            catch (Exception ex)
            {
                string msg = "EXCEPTION "
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n";
                Debug.WriteLine(msg);
                _logger.Error(ex, msg);
                throw;
            }
        }
        private static async Task<IReadOnlyCollection<string>> SaveXmlsReturnPaths(IReadOnlyList<object> meas)
        {
            XmlCreator xmlCreator = new XmlCreator();
            var paths = new SortedSet<string>();
            for (int i = 0; i < meas.Count; i++)
            {
                if (meas[i] is MeasurementView mv)
                {
                    XDocument doc;
                    string file_name;
                    switch (mv.MeasureKind) // MeasurementIndex.Instance.
                    {
                        default: break;
                        case 0:
                            Ddin2Measurement dnm = DbService.Instance.GetDdin2MeasurementById(mv.Id);
                            if (null != dnm)
                            {
                                file_name = CreateName(dnm.Name, dnm.DateTime);
                                doc = xmlCreator.CreateDdin2Xml(dnm);
                                var curr_path = await XmlSaver.SaveXml(file_name, doc);
                                paths.Add(curr_path);
                            }
                            break;
                        case 1:
                            DuMeasurement du;
                            if (0x1201 == mv.MeasureData.Device.Kind)
                            {
                                await DbService.Instance.GetValuesAsync(mv.MeasureData);
                                du = await ConvertToOldMeasurementAsync(mv.MeasureData);
                            }
                            else
                            {
                                du = await DbService.Instance.GetDuMeasurementByIdAsync(mv.Id);
                            }

                            if (null != du)
                            {
                                file_name = CreateName(du.Name, du.DateTime);
                                doc = xmlCreator.CreateDuXml(du);
                                var curr_path = await XmlSaver.SaveXml(file_name, doc);
                                paths.Add(curr_path);
                            }
                            break;
                        case 2:
                            if (0x1700 == mv.MeasureData.Device.Kind)
                                await DbService.Instance.GetValuesAsync(mv.MeasureData);
                            {
                                file_name = CreateName(mv.MeasureData.Device.Name, mv.MeasureData.Measure.BeginTimestamp);
                                doc = xmlCreator.CreateXml(mv.MeasureData);
                                var curr_path = await XmlSaver.SaveXml(file_name, doc);
                                paths.Add(curr_path);
                            }
                            break;
                    }
                }
            }
            string mes_dir = EnvironmentService.Instance.GetDir_Measurements();
            await MediaScannerService.Instance.Scan(mes_dir);
            return paths;
        }
        static async Task<DuMeasurement> ConvertToOldMeasurementAsync(MeasureData mData)
        {
            long sudresearchtype;
            long sudcorrectiontype;
            double lgsoundspeed;
            double bufferpressure;
            double sudpressure;
            long lglevel;
            long lgreflectioncount;

            mData.Measure.TryGet("sudresearchtype", out sudresearchtype);
            mData.Measure.TryGet("sudcorrectiontype", out sudcorrectiontype);
            mData.Measure.TryGet("lgsoundspeed", out lgsoundspeed);
            mData.Measure.TryGet("bufferpressure", out bufferpressure);
            mData.Measure.TryGet("sudpressure", out sudpressure);
            mData.Measure.TryGet("lglevel", out lglevel);
            mData.Measure.TryGet("lgreflectioncount", out lgreflectioncount);

            var secp = new DuMeasurementSecondaryParameters(
                            mData.Device.Name
                            , Resource.Echogram
                            , mData.Position.Field.ToString()
                            , mData.Position.Well.ToString()
                            , mData.Position.Bush.ToString()
                            , mData.Position.Shop.ToString()
                            , bufferpressure
                            , mData.Measure.Comment
                            , "0.0"//_Sensor.Battery
                            , "0.0"//_Sensor.Temperature
                            , "0.0"//_Sensor.Firmware
                            , "0.0"//_Sensor.RadioFirmware
                            , sudresearchtype.ToString()
                            , sudcorrectiontype.ToString()
                            , lgsoundspeed.ToString()
                            );

            var startp = new DuMeasurementStartParameters(false, false, false, secp, 0.0);

            byte[] echoArray = null;
            if (mData.Measure.TryGetBlob("lgechogram", out string echoFile))
            {
                var echoStream = OpenTempFile(echoFile);
                if (null != echoStream)
                {
                    echoArray = new byte[3000];
                    using (echoStream)
                    {
                        await echoStream.ReadAsync(echoArray, 0, (int)echoStream.Length);
                        echoStream.Close();
                    }
                }
            }

            DuMeasurementData data = new DuMeasurementData(mData.Measure.EndTimestamp
                , startp
                , (float)sudpressure
                , (ushort)lglevel
                , (ushort)lgreflectioncount
                , echoArray
                , MeasureState.Ok);

            return new DuMeasurement(data);
        }
        MeasureData Convert(Ddin2Measurement m)
        {
            uint fieldId = 0; uint shop = 0;
            if (Repo.FieldDir.DictByTitle.TryGetValue(m.Field, out FieldItem item))
                fieldId = item.Id;
            uint.TryParse(m.Shop, out shop);
            Position pos = new Position(fieldId, m.Well, m.Bush, shop);
            Models.DeviceInfo dev = new Models.DeviceInfo()
            {
                Kind = 0x1401,
                Name = m.Name,
                Number = uint.MaxValue
            };
            CommonInfo ci = new CommonInfo();
            MeasurementInfo mi = new MeasurementInfo()
            {
                Kind = MeasurementIndex.Instance.GetId(Resource.Dynamogram),
                BeginTimestamp = m.DateTime,
                EndTimestamp = m.DateTime,
                Comment = m.Comment
            };
            var md = new MeasureData(pos, dev, ci, mi);
            md.Id = m.Id;
            return md;
        }
        MeasureData Convert(DuMeasurement m)
        {
            uint fieldId = 0; uint shop = 0;
            if (Repo.FieldDir.DictByTitle.TryGetValue(m.Field, out FieldItem item))
                fieldId = item.Id;
            uint.TryParse(m.Shop, out shop);
            Position pos = new Position(fieldId, m.Well, m.Bush, shop);
            Models.DeviceInfo dev = new Models.DeviceInfo()
            {
                Kind = 0x1101,
                Name = m.Name,
                Number = uint.MaxValue
            };
            CommonInfo ci = new CommonInfo();
            MeasurementInfo mi = new MeasurementInfo()
            {
                Kind = MeasurementIndex.Instance.GetId(Resource.Echogram),
                BeginTimestamp = m.DateTime,
                EndTimestamp = m.DateTime,
                Comment = m.Comment
            };
            var md = new MeasureData(pos, dev, ci, mi);
            md.Id = m.Id;
            return md;
        }


        static FileStream OpenTempFile(string name)
        {
            try
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal), "bin");
                path = Path.Combine(path, name);

                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {

            }
            return null;
        }


        private static bool ValidateForEmptiness()
        {
            List<string> errorList = new List<string>();

            if (string.IsNullOrEmpty(Settings.Instance.FromAddress))
                errorList.Add($"{Resource.EnterFromAddress}");
            if (string.IsNullOrEmpty(Settings.Instance.ToAddress))
                errorList.Add($"{Resource.EnterToAddress}");
            if (string.IsNullOrEmpty(Settings.Instance.SmtpAddress))
                errorList.Add($"{Resource.EnterSmtpAddress}");

            if (Settings.Instance.NeedAuthorization)
            {
                if (string.IsNullOrEmpty(Settings.Instance.Username))
                    errorList.Add($"{Resource.EnterUsername}");
                if (string.IsNullOrEmpty(Settings.Instance.Password))
                    errorList.Add($"{Resource.EnterPassword}");
            }

            if (errorList.Count != 0)
            {
                ShowErrors(errorList);
                return false;
            }

            return true;
        }
        private static string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{DateTimeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }
        private static void ShowErrors(IReadOnlyList<string> errorList)
        {
            if (errorList.Count != 0)
            {
                string errors = "";

                for (int i = 0; i < errorList.Count; i++)
                {
                    errors += errorList[i] + System.Environment.NewLine;
                }

                Application.Current.MainPage
                    .DisplayAlert(Resource.IncorrectDataEnteredErrorText, errors, "OK");
            }
        }
        private static bool CanOpenPage(Type type)
        {
            IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
            if (stack[stack.Count - 1].GetType() != type)
                return true;
            return false;
        }
    }
}
