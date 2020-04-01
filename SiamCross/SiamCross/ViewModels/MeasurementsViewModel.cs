using Autofac;
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
using System.Linq;
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
                        if (CanOpenPage(typeof(Ddim2MeasurementDonePage)))
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
                        if (CanOpenPage(typeof(Ddin2MeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushAsync(
                            new Ddin2MeasurementDonePage(measurement), true);
                        }
                    }
                }
                else if (selectedMeasurement.Name.Contains("SIDDOSA3M"))
                {
                    var measurement = _siddosA3MMeasurements?
                        .SingleOrDefault(m => m.Id == selectedMeasurement.Id);
                    if (measurement != null)
                    {
                        if (CanOpenPage(typeof(SiddosA3MMeasurementDonePage)))
                        {
                            App.NavigationPage.Navigation
                            .PushAsync(
                            new SiddosA3MMeasurementDonePage(measurement), true);
                        }
                    }
                }
                else if (selectedMeasurement.Name.Contains("DU"))
                {
                    var measurement = _duMeasurements?
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

        private bool CanOpenPage(Type type)
        {
            var stack = App.NavigationPage.Navigation.NavigationStack;
            if (stack[stack.Count - 1].GetType() != type)
                return true;
            return false;
        }

        public ObservableCollection<MeasurementView> Measurements { get; set; }

        private List<Ddim2Measurement> _ddim2Measurements;

        private List<Ddin2Measurement> _ddin2Measurements;

        private List<SiddosA3MMeasurement> _siddosA3MMeasurements;

        private List<DuMeasurement> _duMeasurements;

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
            _siddosA3MMeasurements = new List<SiddosA3MMeasurement>();
            _duMeasurements = new List<DuMeasurement>();

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
                            _logger.Error(ex, "Refresh measurement Ddim2" + "\n");
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
                            _logger.Error(ex, "Refresh measurement Ddin2" + "\n");
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
                        _logger.Error(ex, "Refresh measurement SiddosA3M" + "\n");
                    }
                }
            );

            MessagingCenter
                .Subscribe<DuMeasurementDonePage, DuMeasurement>(
                this,
                "Refresh measurement",
                (sender, args) =>
                {
                    try
                    {
                        var mv = Measurements
                            .SingleOrDefault(m => m.Id == args.Id && m.Name == args.Name);
                        if (mv != null)
                        {
                            mv.Field = args.Field;
                            mv.Comments = args.Comment;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Refresh Du measurement" + "\n");
                        throw;
                    }
                });

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
                            _siddosA3MMeasurements.Clear();
                            _duMeasurements.Clear();
                            Measurements.Clear();

                            GetMeasurementsFromDb();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "RefreshAfterDeleting handler lambda" + "\n");
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
                _siddosA3MMeasurements = DataRepository.Instance.GetSiddosA3MMeasurements().ToList();
                _duMeasurements = DataRepository.Instance.GetDuMeasurements().ToList();
                foreach (var m in _ddim2Measurements)
                {
                    Measurements.Add(
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

                foreach (var m in _ddin2Measurements)
                {
                    Measurements.Add(
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

                foreach (var m in _siddosA3MMeasurements)
                {
                    Measurements.Add(
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

                foreach (var m in _duMeasurements)
                {
                    Measurements.Add(
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
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetMeasurementFromDb method" + "\n");
                throw;
            }
        }
    }
}
