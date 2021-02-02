using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Email;
using SiamCross.Services.Environment;
using SiamCross.Services.Logging;
using SiamCross.Services.MediaScanner;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MeasurementsVM : BaseViewModel, IViewModel
    {
        private string _title;
        private List<Ddin2Measurement> _ddin2Measurements;
        private List<DuMeasurement> _duMeasurements;
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private bool _selectMode = false;

        public bool SelectMode
        {
            get => _selectMode;
            set
            {
                _selectMode = value;
                NotifyPropertyChanged(nameof(SelectMode));
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged(nameof(Title));
                NotifyPropertyChanged(nameof(SelCount));
            }
        }
        public int SelCount => SelectedMeasurements.Count;

        public ObservableCollection<MeasurementView> Measurements { get; }
        public ObservableCollection<object> SelectedMeasurements { get; }

        public ICommand RefreshCommand { get; set; }
        public ICommand SelectAllCommand { get; set; }
        public ICommand UnselectAllCommand { get; set; }

        public ICommand ShareCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ICommand OnItemLongPressCommand { get; set; }
        public ICommand OnItemTappedCommand { get; set; }

        public MeasurementsVM()
        {
            Measurements = new ObservableCollection<MeasurementView>();
            SelectedMeasurements = new ObservableCollection<object>();
            Title = Resource.MeasurementsTitle;
            RefreshCommand = new Command(ReloadMeasurementsFromDb);
            SelectAllCommand = new Command(SelectAll);
            UnselectAllCommand = new Command(UnselectAll);
            ShareCommand = new Command(ShareMeasurementsAsync);
            SendCommand = new Command(SendMeasurementsAsync);
            SaveCommand = new Command(SaveMeasurementsAsync);
            DeleteCommand = new Command(DeleteMeasurementsAsync);

            OnItemTappedCommand = new Command(obj => OnItemTapped(obj as MeasurementView));
            OnItemLongPressCommand = new Command(obj => OnItemLongPress(obj as MeasurementView));

        }
        public void ReloadMeasurementsFromDb()
        {
            try
            {
                SelectedMeasurements.Clear();
                Measurements.Clear();
                ObservableCollection<MeasurementView> meas = new ObservableCollection<MeasurementView>();
                _ddin2Measurements = DataRepository.Instance.GetDdin2Measurements().ToList();
                _duMeasurements = DataRepository.Instance.GetDuMeasurements().ToList();

                foreach (Ddin2Measurement m in _ddin2Measurements)
                {
                    meas.Add(
                        new MeasurementView
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Field = m.Field,
                            Date = m.DateTime,
                            MeasurementType = Resource.Dynamogram,
                            Comments = m.Comment
                        });
                }

                foreach (DuMeasurement m in _duMeasurements)
                {
                    meas.Add(
                        new MeasurementView
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Field = m.Field,
                            Date = m.DateTime,
                            MeasurementType = Resource.Echogram,
                            Comments = m.Comment
                        });
                }

                foreach (MeasurementView element in meas.OrderByDescending(m => m.Date))
                {
                    Measurements.Add(element);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetMeasurementFromDb method" + "\n");
                throw;
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
            Title = $"{Resource.SelectedMeasurements}: {SelCount}";
        }
        public void OnItemTapped(MeasurementView item)
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
                PushPage(item);
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
            IEnumerable<MeasurementView> selected = Measurements.Where(element => element.IsSelected);
            foreach (MeasurementView item in selected)
                item.IsSelected = false;

        }
        private void SelectAll()
        {
            if (!SelectMode)
                SelectMode = true;
            IEnumerable<MeasurementView> selected = Measurements.Where(element => !element.IsSelected);
            foreach (MeasurementView item in selected)
                item.IsSelected = true;
        }
        private async void ShareMeasurementsAsync(object obj)
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                string[] paths = await SaveXmlsReturnPaths(SelectedMeasurements);

                List<ShareFile> sf = new List<ShareFile>();
                for (int i = 0; i < paths.Length; ++i)
                {
                    sf.Add(new ShareFile(paths[i]));
                }
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = Title,
                    Files = sf
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler" + "\n");
                throw;
            }
        }
        private async void SendMeasurementsAsync(object obj)
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                if (!ValidateForEmptiness())
                    return;

                foreach (MeasurementView survay in SelectedMeasurements)
                {
                    survay.Sending = true;
                    survay.LastSentRecipient = SiamCross.Models.Tools.Settings.Instance.ToAddress;
                }

                //ToastService.Instance.LongAlert($"{Resource.SendingMeasurements}...");
                string[] paths = await SaveXmlsReturnPaths(SelectedMeasurements);

                bool is_ok = await EmailService.Instance.SendEmailWithFilesAsync("Siam Measurements",
                    "\nSiamCompany Telemetry Transfer Service",
                    paths);
                //ToastService.Instance.LongAlert($"{SelectedMeasurements.Count} {Resource.MeasurementsSentSuccesfully}");

                foreach (MeasurementView sent_sur in SelectedMeasurements)
                {
                    sent_sur.LastSentTimestamp = DateTime.Now.ToString();
                    sent_sur.LastSentRecipient = SiamCross.Models.Tools.Settings.Instance.ToAddress;
                    sent_sur.Sending = false;
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
                    err_sur.Sending = false;
                }
            }
        }
        private async void SaveMeasurementsAsync(object obj)
        {
            try
            {
                if (0 == SelectedMeasurements.Count)
                    return;
                //ToastService.Instance.LongAlert($"{Resource.SavingMeasurements}...");
                foreach (MeasurementView survay in SelectedMeasurements)
                    survay.Saving = true;

                string[] paths = await SaveXmlsReturnPaths(SelectedMeasurements);

                string ts = DateTime.Now.ToString();
                string dir = EnvironmentService.Instance.GetDir_Measurements();
                foreach (MeasurementView m in SelectedMeasurements)
                {
                    m.Saving = false;
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
                throw;
            }
        }
        private async void DeleteMeasurementsAsync()
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
                            if (mv.Name.Contains("DDIM")
                                || mv.Name.Contains("DDIN")
                                || mv.Name.Contains("SIDDOSA3M"))
                            {
                                DataRepository.Instance.RemoveDdin2Measurement(mv.Id);
                                Measurements.Remove(mv);
                            }
                            else if (mv.Name.Contains("DU"))
                            {
                                DataRepository.Instance.RemoveDuMeasurement(mv.Id);
                                Measurements.Remove(mv);
                            }
                        }
                    }
                    //MessagingCenter.Send(this, "RefreshAfterDeleting");
                    SelectedMeasurements.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteMeasurements method" + "\n");
                throw;
            }
        }
        public void PushPage(MeasurementView selectedMeasurement)
        {
            if (null == selectedMeasurement)
                return;
            UnselectAll();

            try
            {
                if (selectedMeasurement.Name.Contains("DDIM")
                    || selectedMeasurement.Name.Contains("DDIN")
                    || selectedMeasurement.Name.Contains("SIDDOSA3M")
                    )
                {
                    Ddin2Measurement measurement = _ddin2Measurements?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenPage(typeof(Ddin2MeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushAsync(
                            new Ddin2MeasurementDonePage(measurement), true);
                        }
                    }
                }
                else if (selectedMeasurement.Name.Contains("DU"))
                {
                    DuMeasurement measurement = _duMeasurements?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenPage(typeof(DuMeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                                .PushAsync(
                                    new DuMeasurementDonePage(measurement), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PushPage method" + "\n");
                throw;
            }
        }
        private static async Task<string[]> SaveXmlsReturnPaths(IReadOnlyList<object> meas)
        {
            XmlCreator xmlCreator = new XmlCreator();
            string[] paths = new string[meas.Count];
            for (int i = 0; i < meas.Count; i++)
            {
                if (meas[i] is MeasurementView mv)
                {
                    string file_name = null;
                    XDocument doc = null;

                    if (mv.Name.Contains("DDIM") || mv.Name.Contains("DDIN") || mv.Name.Contains("SIDDOSA3M"))
                    {
                        Ddin2Measurement dnm = DataRepository.Instance.GetDdin2MeasurementById(mv.Id);
                        file_name = CreateName(dnm.Name, dnm.DateTime);
                        doc = xmlCreator.CreateDdin2Xml(dnm);
                    }
                    else if (mv.Name.Contains("DU"))
                    {
                        //Get siddos by id
                        DuMeasurement du = DataRepository.Instance.GetDuMeasurementById(mv.Id);
                        file_name = CreateName(du.Name, du.DateTime);
                        doc = xmlCreator.CreateDuXml(du);
                    }
                    paths[i] = await XmlSaver.SaveXml(file_name, doc);
                }
            }

            string mes_dir = EnvironmentService.Instance.GetDir_Measurements();
            await MediaScannerService.Instance.Scan(mes_dir);
            return paths;
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

            if (Settings.Instance.IsNeedAuthorization)
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
