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
                await Task.Run(() =>
                {
                    foreach (var m in SelectedMeasurements)
                    {
                        if (m is MeasurementView mv)
                        {
                            EmailService.Instance.SendEmail
                                ("", mv.Name + mv.MeasurementType, mv.MeasurementType + mv.Date.ToString());
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                ex.Message, "OK");
            }
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
            ValidateParameter(Settings.Instance.Username,
                "Введите имя пользователя!");
            ValidateParameter(Settings.Instance.Password,
                "Введите пароль!");

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
