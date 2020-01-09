using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class MeasurementsViewModel : BaseViewModel, IViewModel
    {
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

        private List<Ddim2Measurement> _ddim2Measurements;

        private List<Ddin2Measurement> _ddin2Measurements;

        public ICommand SelectAll { get; set; }

        public MeasurementsViewModel()
        {
            SelectAll = new Command(() =>
            {
                App.NavigationPage
                    .Navigation
                    .PushAsync(new MeasurementsSelectionPage(Measurements));
                App.MenuIsPresented = false;
            });
            Measurements = new ObservableCollection<MeasurementView>();
            _ddim2Measurements = new List<Ddim2Measurement>();
            _ddin2Measurements = new List<Ddin2Measurement>();

            //var ddim2M = new Ddim2Measurement()
            //{
            //    AccelerationGraph = null,
            //    ApertNumber = 1,
            //    BufferPressure = "5",
            //    Bush = "1",
            //    Field = "Первое поле",
            //    Comment = "Заедание на плунжерном насосе",
            //    DateTime = DateTime.Now,
            //    Id = 0,
            //    DynGraph = new byte[] { 2, 3 },
            //    Well = "2",
            //    Period = 20,
            //    ModelPump = 0,
            //    ErrorCode = "100",
            //    Shop = "2",
            //    Name = "DDIM0170",
            //    Step = 50,
            //    MaxBarbellWeight = 10,
            //    MaxWeight = 100,
            //    MinBarbellWeight = 5,
            //    MinWeight = 50,
            //    TimeDiscr = 50,
            //    Travel = 100,
            //    WeightDiscr = 15
            //};

            //var ddin2M = new Ddin2Measurement()
            //{
            //    AccelerationGraph = null,
            //    ApertNumber = 1,
            //    BufferPressure = "5",
            //    Bush = "1",
            //    Field = "Первое поле",
            //    Comment = "Заедание на плунжерном насосе",
            //    DateTime = DateTime.Now,
            //    Id = 1,
            //    DynGraph = new byte[] { 2, 3 },
            //    Well = "2",
            //    Period = 20,
            //    ModelPump = 0,
            //    ErrorCode = "100",
            //    Shop = "2",
            //    Name = "DDIN0584",
            //    Step = 50,
            //    MaxBarbellWeight = 10,
            //    MaxWeight = 100,
            //    MinBarbellWeight = 5,
            //    MinWeight = 50,
            //    TimeDiscr = 50,
            //    Travel = 100,
            //    WeightDiscr = 15
            //};

            //_ddim2Measurements.Add(ddim2M);
            //_ddin2Measurements.Add(ddin2M);

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

            GetMeasurementsFromDb();

            Measurements.OrderBy(m => m.Date);

            MessagingCenter
                .Subscribe<Ddim2MeasurementDonePage, Ddim2Measurement>(
                    this, 
                    "Refresh measurement", 
                    (sender, arg)=>
                    {
                        var mv = Measurements.SingleOrDefault(m => m.Id == arg.Id);
                        if (mv != null)
                        {
                            mv.Field = arg.Field;
                            mv.Comments = arg.Comment;
                        }
                    }
                );

            MessagingCenter
                .Subscribe<Ddin2MeasurementDonePage, Ddin2Measurement>(
                    this,
                    "Refresh measurement",
                    (sender, arg) =>
                    {
                        var mv = Measurements.SingleOrDefault(m => m.Id == arg.Id);
                        if (mv != null)
                        {
                            mv.Field = arg.Field;
                            mv.Comments = arg.Comment;
                        }
                    }
                );

            MessagingCenter
                .Subscribe<MeasurementsSelectionViewModel>(
                    this, 
                    "RefreshAfterDeleting", 
                    (sender)=> 
                    {
                        _ddim2Measurements.Clear();
                        _ddin2Measurements.Clear();
                        Measurements.Clear();

                        GetMeasurementsFromDb();
                    }
                );
        }

        private void GetMeasurementsFromDb()
        {
            _ddim2Measurements = DataRepository.Instance.GetDdim2Items().ToList();
            _ddin2Measurements = DataRepository.Instance.GetDdin2Items().ToList();
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
                        Name = m.Name,
                        Field = m.Field,
                        Date = m.DateTime,
                        MeasurementType = "Динамограмма",
                        Comments = m.Comment
                    });
            }
        }
    }
}
