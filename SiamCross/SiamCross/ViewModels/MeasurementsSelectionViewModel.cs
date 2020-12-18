using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class MeasurementsSelectionViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private List<string> _errorList;
        private DateTimeConverter _timeConverter = new DateTimeConverter();
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
            foreach (var m in measurements)
            {
                Measurements.Add(m);
                //SelectedMeasurements.Add(m);
            }
            _errorList = new List<string>();
            Title = $"{Resource.SelectedMeasurements}: {SelectedMeasurements.Count}";
            SendCommand = new Command(ShareMeasurements);
            DeleteCommand = new Command(DeleteMeasurements);
            SelectionChanged = new Command(RefreshSelectedCount);
            SendCommand = new Command(SendMeasurements);
        }
        public async void SaveMeasurements(object obj)
        {
            try
            {
                DependencyService.Get<IToast>().Show($"{Resource.SavingMeasurements}...");

                await Task.Run(() =>
                {
                    var paths = SaveXmlsReturnPaths();
                });

                var savePath = @"""Measurements""";

                DependencyService.Get<IToast>()
                    .Show($"{SelectedMeasurements.Count} " +
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
                    foreach (var m in SelectedMeasurements)
                    {
                        if (m is MeasurementView mv)
                        {
                            if (mv.Name.Contains("DDIM"))
                            {
                                DataRepository.Instance.RemoveDdim2Measurement(mv.Id);
                                Measurements.Remove(mv);
                            }
                            else if (mv.Name.Contains("DDIN"))
                            {
                                DataRepository.Instance.RemoveDdin2Measurement(mv.Id);
                                Measurements.Remove(mv);
                            }
                            else if (mv.Name.Contains("SIDDOSA3M"))
                            {
                                DataRepository.Instance.RemoveSiddosA3MMeasurement(mv.Id);
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


                DependencyService.Get<IToast>().Show($"{Resource.SendingMeasurements}...");
                var paths = await SaveXmlsReturnPaths();

                bool is_ok = await EmailService.Instance.SendEmailWithFiles("Siam Measurements",
                    "\nSiamCompany Telemetry Transfer Service",
                    paths);
                DependencyService.Get<IToast>()
                    .Show($"{SelectedMeasurements.Count} {Resource.MeasurementsSentSuccesfully}");

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
        private void ShareMeasurements(object obj)
        {
            try
            {
                /*
                 Waiting Essentional 1.6
                var paths = SaveXmlsReturnPaths();
                ShareFile[] sf = new ShareFile[paths.Length];
                for (int i = 0; i < paths.Length; ++i)
                {
                    sf[i] = new ShareFile(paths[i]);
                }
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = Title,
                    Files = sf
                });
                */
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler" + "\n");
                throw;
            }
        }
        private async Task<string[]> SaveXmlsReturnPaths()
        {
            var xmlCreator = new XmlCreator();
            var xmlSaver = DependencyService.Get<IXmlSaver>();
            var paths = new string[SelectedMeasurements.Count];
            for (int i = 0; i < SelectedMeasurements.Count; i++)
            {
                if (SelectedMeasurements[i] is MeasurementView mv)
                {
                    if (   mv.Name.Contains("DDIM")
                        || mv.Name.Contains("DDIN")
                        || mv.Name.Contains("SIDDOSA3M") )
                    {
                        var dnm = DataRepository.Instance.GetDdin2MeasurementById(mv.Id);
                        var name = CreateName(dnm.Name, dnm.DateTime);
                        bool saved = await xmlSaver.SaveXml(name, xmlCreator.CreateDdin2Xml(dnm));
                        if (saved)
                            paths[i] = xmlSaver.GetFilepath(name);
                    }
                    else if (mv.Name.Contains("DU"))
                    {
                        //Get siddos by id
                        var du = DataRepository.Instance.GetDuMeasurementById(mv.Id);
                        var name = CreateName(du.Name, du.DateTime);
                        XDocument doc = xmlCreator.CreateDuXml(du);
                        bool saved = await xmlSaver.SaveXml(name, doc);
                        if(saved)
                            paths[i] = xmlSaver.GetFilepath(name);
                    }
                }
            }
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
                    errors += _errorList[i] + Environment.NewLine;
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
