﻿using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using SiamCross.Services.Logging;
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
        public void PushPage(MeasurementView selectedMeasurement)
        {
            try
            {
                if (selectedMeasurement.Name.Contains("DDIM"))
                {
                    var measurement = _ddim2Measurements?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenModalPage(typeof(Ddim2MeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushAsync(
                            new Ddim2MeasurementDonePage(measurement), true);
                        }
                    }
                }
                else if (selectedMeasurement.Name.Contains("DDIN"))
                {
                    var measurement = _ddin2Measurements?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenModalPage(typeof(Ddin2MeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushModalAsync(
                            new Ddin2MeasurementDonePage(measurement), true);
                        }
                    }
                }
                else if (selectedMeasurement.Name.Contains("SIDDOSA3M"))
                {
                    var measurement = _siddosA3MMeasurement?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenModalPage(typeof(SiddosA3MMeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushModalAsync(
                            new SiddosA3MMeasurementDonePage(measurement), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PushPage method");
                throw;
            }
        }

        private bool CanOpenModalPage(Type type)
        {
            bool result = false;
            var stack = App.NavigationPage.Navigation.ModalStack;

            if (stack.Count > 0)
            {
                if (stack[stack.Count - 1].GetType() != type)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public ObservableCollection<MeasurementView> Measurements { get; set; }

        private List<Ddim2Measurement> _ddim2Measurements;

        private List<Ddin2Measurement> _ddin2Measurements;

        private List<SiddosA3MMeasurement> _siddosA3MMeasurement;

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

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
            _siddosA3MMeasurement = new List<SiddosA3MMeasurement>();

            #region Comment
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
            #endregion

            GetMeasurementsFromDb();

            List<MeasurementView> ordered = Measurements.OrderByDescending(m => m.Date).ToList();
            Measurements.Clear();
            foreach (var m in ordered)
            {
                Measurements.Add(m);
            }

            MessagingCenter
                .Subscribe<Ddim2MeasurementDonePage, Ddim2Measurement>(
                    this,
                    "Refresh measurement",
                    (sender, arg) =>
                    {
                        try
                        {
                            var mv = Measurements
                                .SingleOrDefault(m => m.Id == arg.Id && m.Name == arg.Name);
                            if (mv != null)
                            {
                                mv.Field = arg.Field;
                                mv.Comments = arg.Comment;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Refresh measurement Ddim2");
                        }
                    }
                );

            MessagingCenter
                .Subscribe<Ddin2MeasurementDonePage, Ddin2Measurement>(
                    this,
                    "Refresh measurement",
                    (sender, arg) =>
                    {
                        try
                        {
                            var mv = Measurements
                            .SingleOrDefault(m => m.Id == arg.Id && m.Name == arg.Name);
                            if (mv != null)
                            {
                                mv.Field = arg.Field;
                                mv.Comments = arg.Comment;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Refresh measurement Ddin2");
                        }
                    }
                );

            MessagingCenter
            .Subscribe<SiddosA3MMeasurementDonePage, SiddosA3MMeasurement>(
                this,
                "Refresh measurement",
                (sender, arg) =>
                {
                    try
                    {
                        var mv = Measurements
                            .SingleOrDefault(m => m.Id == arg.Id && m.Name == arg.Name);
                        if (mv != null)
                        {
                            mv.Field = arg.Field;
                            mv.Comments = arg.Comment;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Refresh measurement SiddosA3M");
                    }
                }
            );

            MessagingCenter
                .Subscribe<MeasurementsSelectionViewModel>(
                    this,
                    "RefreshAfterDeleting",
                    (sender) =>
                    {
                        try
                        {
                            _ddim2Measurements.Clear();
                            _ddin2Measurements.Clear();
                            Measurements.Clear();

                            GetMeasurementsFromDb();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "RefreshAfterDeleting handler lambda");
                        }
                    }
                );
        }

        private void GetMeasurementsFromDb()
        {
            try
            {
                _ddim2Measurements = DataRepository.Instance.GetDdim2Measurements().ToList();
                _ddin2Measurements = DataRepository.Instance.GetDdin2Measurements().ToList();
                _siddosA3MMeasurement = DataRepository.Instance.GetSiddosA3MMeasurements().ToList();
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

                foreach (var m in _siddosA3MMeasurement)
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
            catch (Exception ex)
            {
                _logger.Error(ex, "GetMeasurementFromDb method");
                throw;
            }
        }
    }
}
