using SiamCross.DataBase;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
        public MeasurementsSelectionViewModel(ObservableCollection<MeasurementView> measurements)
        {
            Measurements = new ObservableCollection<MeasurementView>();
            SelectedMeasurements = new ObservableCollection<object>();
            foreach (var m in measurements)
            {
                Measurements.Add(m);
                SelectedMeasurements.Add(m);
            }
            Title = $"Выбрано: {SelectedMeasurements.Count}";
            DeleteCommand = new Command(DeleteMeasurements);
            SelectionChanged = new Command(RefreshSelectedCount);
        }

        private void RefreshSelectedCount()
        {
            Title = $"Выбрано: {SelectedMeasurements.Count}";
        }

        private void DeleteMeasurements()
        {
            if (SelectedMeasurements.Count != 0)
            {
                var dataBase = new RealmDBController();
                foreach (var m in SelectedMeasurements)
                {
                    if (m is MeasurementView mv)
                    {
                        if (mv.Name.Contains("DDIM"))
                        {
                            dataBase.RemoveDdim2FromId(mv.Id);
                            Measurements.Remove(mv);
                        }
                        else if (mv.Name.Contains("DDIN"))
                        {
                            dataBase.RemoveDdim2FromId(mv.Id);
                            Measurements.Remove(mv);
                        }
                    }
                }
                MessagingCenter.Send<MeasurementsSelectionViewModel>(this, "RefreshAfterDeleting");
                SelectedMeasurements.Clear();
                RefreshSelectedCount();
            }
        }
    }
}
