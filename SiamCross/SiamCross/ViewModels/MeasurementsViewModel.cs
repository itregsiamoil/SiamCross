using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SiamCross.ViewModels
{
    public class MeasurementsViewModel : BaseViewModel, IViewModel
    {
        public class MeasurementView
        {
            public int Id { get; set; }

            public string Name { get; set; }
            public string Field { get; set; }
            public DateTime Date { get; set; }
            public string MeasurementType { get; set; }
            public string Comments { get; set; }
        }

        private MeasurementView _selectedMeasurement;

        public MeasurementView SelectedMeasurement
        {
            get => _selectedMeasurement;
            set
            {
                _selectedMeasurement = value;

                if (_selectedMeasurement == null) return;

                PushPage();

                SelectedMeasurement = null;
            }
        }

        private void PushPage()
        {
            if (_selectedMeasurement.Name.Contains("DDIM"))
            {
                var measurement = _ddim2Measurements?
                    .SingleOrDefault(m => m.Id == _selectedMeasurement.Id);
                if (measurement != null)
                {
                    App.NavigationPage.Navigation
                        .PushModalAsync(
                        new Ddim2MeasurementDonePage(measurement), true);
                }
            }
            else if (_selectedMeasurement.Name.Contains("DDIN"))
            {
                var measurement = _ddin2Measurements?
                    .SingleOrDefault(m => m.Id == _selectedMeasurement.Id);
                if (measurement != null)
                {
                    App.NavigationPage.Navigation
                        .PushModalAsync(
                        new Ddin2MeasurementDonePage(measurement), true);
                }
            }
        }

        public ObservableCollection<MeasurementView> Measurements { get; set; }

        private IEnumerable<Ddim2Measurement> _ddim2Measurements;

        private IEnumerable<Ddin2Measurement> _ddin2Measurements;

        public MeasurementsViewModel()
        {
            Measurements = new ObservableCollection<MeasurementView>();
            _ddim2Measurements = DataRepository.Instance.GetDdim2Items();
            _ddin2Measurements = DataRepository.Instance.GetDdin2Items();

            //Measurements.Add(
            //        new MeasurementView
            //        {
            //            Id = 0,
            //            Field = "Первое поле",
            //            Date = DateTime.Now,
            //            MeasurementType = "Динамограмма",
            //            Comments = "Комментарии вавыафыафыафывафыва fsfgsdfgsdfgsdfg вавыафыафыафывафыва Комментарии вавыафыафыафывафыва Комментарии вавыафыафыафывафыва"
            //        });

            //Measurements.Add(
            //        new MeasurementView
            //        {
            //            Id = 1,
            //            Field = "Второе поле",
            //            Date = DateTime.Now,
            //            MeasurementType = "Динамограмма",
            //            Comments = "Комментарии вавыафыафыафывафыва"
            //        });

            foreach (var m in _ddim2Measurements)
            {
                Measurements.Add(
                    new MeasurementView
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Field = m.Field,
                        Date = m.DateTime,
                        MeasurementType = "Динамограмма",
                        Comments = m.Comment
                    });
            }

            foreach (var m in _ddin2Measurements)
            {
                Measurements.Add(
                    new MeasurementView
                    {
                        Id = m.Id,
                        Field = m.Field,
                        Date = m.DateTime,
                        MeasurementType = "Динамограмма",
                        Comments = m.Comment
                    });
            }


            Measurements.OrderBy(m => m.Date);
        }
    }
}
