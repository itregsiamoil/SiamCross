using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class MeasurementsSelectionViewModel : BaseViewModel, IViewModel
    {
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
                SelectedMeasurements.Add(m);
            }
            _errorList = new List<string>();
            Title = $"Выбрано: {SelectedMeasurements.Count}";
            DeleteCommand = new Command(DeleteMeasurements);
            SelectionChanged = new Command(RefreshSelectedCount);
            SendCommand = new Command(SendMeasurements);
        }

        public async void SaveMeasurements(object obj)
        {
            try
            {
                DependencyService.Get<IToast>().Show("Сохранение измерений...");

                await Task.Run(() =>
                {
                    var paths = SaveXmlsReturnPaths();
                });

                var savePath = @"""Download\Measurements""";

                DependencyService.Get<IToast>()
                    .Show($"{SelectedMeasurements.Count} " +
                    $"измерений успешно сохранены в {savePath}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                ex.Message, "OK");
            }
        }

        private void RefreshSelectedCount()
        {
            Title = $"Выбрано: {SelectedMeasurements.Count}";
        }

        private void DeleteMeasurements()
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
                    }
                }
                MessagingCenter.Send<MeasurementsSelectionViewModel>(this, "RefreshAfterDeleting");
                SelectedMeasurements.Clear();
                RefreshSelectedCount();
            }
        }

        private List<string> _errorList;

        private async void SendMeasurements(object obj)
        {
            if (!ValidateForEmptiness())
            {
                return;
            }

            try
            {
                DependencyService.Get<IToast>().Show("Отправка измерений на почту");

                await Task.Run(() =>
                {
                    var paths = SaveXmlsReturnPaths();

                    EmailService.Instance.SendEmailWithFiles("Siam Measurements",
                        "Hello!\n\nSiamCompany Telemetry Transfer Service", 
                        paths);
                });

                DependencyService.Get<IToast>()
                    .Show($"{SelectedMeasurements.Count} измерений успешно отправлено на почту");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                ex.Message, "OK");
            }
        }

        private DateTimeConverter _timeConverter = new DateTimeConverter();

        private string[] SaveXmlsReturnPaths()
        {
            var xmlCreator = new XmlCreator();

            var xmlSaver = DependencyService.Get<IXmlSaver>();

            var paths = new string[SelectedMeasurements.Count];

            for (int i = 0; i < SelectedMeasurements.Count; i++)
            {
                if (SelectedMeasurements[i] is MeasurementView mv)
                {
                    if (mv.Name.Contains("DDIM"))
                    {
                        var dmm = DataRepository.Instance.GetDdim2MeasurementById(mv.Id);
                        var name = CreateName(dmm.Name, dmm.DateTime);
                        xmlSaver.SaveXml(name, xmlCreator.CreateDdim2Xml(dmm));

                        paths[i] = xmlSaver.GetFilepath(name);
                    }
                    else if (mv.Name.Contains("DDIN"))
                    {
                        var dnm = DataRepository.Instance.GetDdin2MeasurementById(mv.Id);
                        var name = CreateName(dnm.Name, dnm.DateTime);
                        xmlSaver.SaveXml(name, xmlCreator.CreateDdin2Xml(dnm));

                        paths[i] = xmlSaver.GetFilepath(name);
                    }
                    else if (mv.Name.Contains("SIDDOSA3M"))
                    {
                        //Get siddos by id
                        var sdm = DataRepository.Instance.GetSiddosA3MMeasurementById(mv.Id);
                        var name = CreateName(sdm.Name, sdm.DateTime);
                        xmlSaver.SaveXml(name, xmlCreator.CreateSiddosA3MXml(sdm));
                    }
                }
            }

            return paths;
        }

        private string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{_timeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }

        private bool ValidateForEmptiness()
        {
            _errorList.Clear();

            ValidateParameter(Settings.Instance.FromAddress, 
                "Введите адресат отправителя!");
            ValidateParameter(Settings.Instance.ToAddress,
                "Введите адресат назначения!");
            ValidateParameter(Settings.Instance.SmtpAddress,
                "Введите адрес SMTP сервера!");

            if (Settings.Instance.NeedAuthorization)
            {
                ValidateParameter(Settings.Instance.Username,
                "Введите имя пользователя!");
                ValidateParameter(Settings.Instance.Password,
                    "Введите пароль!");
            }
            

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
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

                Application.Current.MainPage.DisplayAlert("Введены неправильные данные",
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
