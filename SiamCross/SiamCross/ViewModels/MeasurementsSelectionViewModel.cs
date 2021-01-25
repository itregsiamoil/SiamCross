using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Email;
using SiamCross.Services.Environment;
using SiamCross.Services.Logging;
using SiamCross.Services.MediaScanner;
using SiamCross.Services.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MeasurementsSelectionViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private readonly List<string> _errorList;
        private readonly DateTimeConverter _timeConverter = new DateTimeConverter();
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged(nameof(Title));
            }
        }
        public ObservableCollection<MeasurementView> Measurements { get; set; }
        public ObservableCollection<object> SelectedMeasurements { get; set; }
        public ICommand ShareCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SelectionChanged { get; set; }
        public ICommand SendCommand { get; set; }

        public MeasurementsSelectionViewModel(ObservableCollection<MeasurementView> measurements)
        {
            Measurements = new ObservableCollection<MeasurementView>();
            SelectedMeasurements = new ObservableCollection<object>();
            foreach (MeasurementView m in measurements)
            {
                Measurements.Add(m);
                //SelectedMeasurements.Add(m);
            }
            _errorList = new List<string>();
            Title = $"{Resource.SelectedMeasurements}: {SelectedMeasurements.Count}";
            DeleteCommand = new Command(DeleteMeasurements);
            SelectionChanged = new Command(RefreshSelectedCount);
            SendCommand = new Command(SendMeasurements);
        }
        public async void SaveMeasurements(object obj)
        {
            try
            {
                ToastService.Instance.LongAlert($"{Resource.SavingMeasurements}...");

                await Task.Run(() =>
                {
                    Task<string[]> paths = SaveXmlsReturnPaths();
                });

                string savePath = @"""Measurements""";

                ToastService.Instance.LongAlert($"{SelectedMeasurements.Count} " +
                    $"{Resource.MeasurementsSavedSuccesfully} {savePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveMeasurements method" + "\n");
                await Application.Current.MainPage.DisplayAlert(Resource.Error,
                ex.Message, "OK");
                throw;
            }
        }
        private void RefreshSelectedCount()
        {
            Title = $"{Resource.SelectedMeasurements}: {SelectedMeasurements.Count}";
            foreach (MeasurementView m in Measurements)
            {
                m.IsSelected = false;
            }
            foreach (MeasurementView m in SelectedMeasurements)
            {
                m.IsSelected = true;
            }

        }
        private void DeleteMeasurements()
        {
            try
            {
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
                    MessagingCenter.Send<MeasurementsSelectionViewModel>(this, "RefreshAfterDeleting");
                    SelectedMeasurements.Clear();
                    RefreshSelectedCount();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteMeasurements method" + "\n");
                throw;
            }
        }
        private async void SendMeasurements(object obj)
        {
            if (!ValidateForEmptiness())
            {
                return;
            }

            //Присваиваем команде пустую лямбду
            SendCommand = new Command(() => { });

            try
            {
                foreach (MeasurementView survay in SelectedMeasurements)
                {
                    survay.Sending = true;
                    survay.LastSentRecipient = SiamCross.Models.Tools.Settings.Instance.ToAddress;
                }

                ToastService.Instance.LongAlert($"{Resource.SendingMeasurements}...");
                string[] paths = await SaveXmlsReturnPaths();

                bool is_ok = await EmailService.Instance.SendEmailWithFilesAsync("Siam Measurements",
                    "\nSiamCompany Telemetry Transfer Service",
                    paths);
                ToastService.Instance.LongAlert($"{SelectedMeasurements.Count} {Resource.MeasurementsSentSuccesfully}");

                foreach (MeasurementView sent_sur in SelectedMeasurements)
                {
                    sent_sur.SetLastSentTimestamp(DateTime.Now);
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
                    err_sur.SetLastSentTimestamp(new DateTime(0));
                    err_sur.LastSentRecipient = "";
                    err_sur.Sending = false;
                }
            }
            SendCommand = new Command(SendMeasurements);

        }
        private async void ShareMeasurementsAsync(object obj)
        {
            try
            {
                string ShareFilesTitle = "ShareMultiple";
                string[] paths = await SaveXmlsReturnPaths();

                List<ShareFile> sf = new List<ShareFile>();
                for (int i = 0; i < paths.Length; ++i)
                {
                    sf.Add(new ShareFile(paths[i]));
                }
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = ShareFilesTitle,
                    Files = sf
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler" + "\n");
                throw;
            }
        }
        private async Task<string[]> SaveXmlsReturnPaths()
        {
            XmlCreator xmlCreator = new XmlCreator();
            string[] paths = new string[SelectedMeasurements.Count];
            for (int i = 0; i < SelectedMeasurements.Count; i++)
            {
                if (SelectedMeasurements[i] is MeasurementView mv)
                {
                    string file_name = null;
                    XDocument doc = null;

                    if (mv.Name.Contains("DDIM") || mv.Name.Contains("DDIN") || mv.Name.Contains("SIDDOSA3M"))
                    {
                        DataBase.DataBaseModels.Ddin2Measurement dnm = DataRepository.Instance.GetDdin2MeasurementById(mv.Id);
                        file_name = CreateName(dnm.Name, dnm.DateTime);
                        doc = xmlCreator.CreateDdin2Xml(dnm);
                    }
                    else if (mv.Name.Contains("DU"))
                    {
                        //Get siddos by id
                        DataBase.DataBaseModels.DuMeasurement du = DataRepository.Instance.GetDuMeasurementById(mv.Id);
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
        private bool ValidateForEmptiness()
        {
            _errorList.Clear();

            ValidateParameter(Settings.Instance.FromAddress,
                $"{Resource.EnterFromAddress}");
            ValidateParameter(Settings.Instance.ToAddress,
                $"{Resource.EnterToAddress}!");
            ValidateParameter(Settings.Instance.SmtpAddress,
                $"{Resource.EnterSmtpAddress}!");

            if (Settings.Instance.IsNeedAuthorization)
            {
                ValidateParameter(Settings.Instance.Username,
                $"{Resource.EnterUsername}!");
                ValidateParameter(Settings.Instance.Password,
                    $"{Resource.EnterPassword}!");
            }


            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }
        private string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{_timeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }
        private void ShowErrors()
        {
            if (_errorList.Count != 0)
            {
                string errors = "";

                for (int i = 0; i < _errorList.Count; i++)
                {
                    errors += _errorList[i] + System.Environment.NewLine;
                }

                Application.Current.MainPage.DisplayAlert(Resource.IncorrectDataEnteredErrorText,
                errors, "OK");
            }
        }
        private void ValidateParameter(string text, string errorMessage)
        {
            if (string.IsNullOrEmpty(text))
            {
                _errorList.Add(errorMessage);
            }
        }
    }
}
