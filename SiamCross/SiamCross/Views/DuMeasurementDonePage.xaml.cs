using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DuMeasurementDonePage : ContentPage
    {
        private static readonly Logger _logger = 
            AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly DuMeasurement _measurement;

        public DuMeasurementDonePage(DuMeasurement measurement)
        {
            try
            {
                _measurement = measurement;
                var vmWrap = new ViewModelWrap<DuMeasurementDoneViewModel>(measurement);
                this.BindingContext = vmWrap.ViewModel;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DuMeasurementDonePage ctor");
                throw;
            }
        }

        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                DataRepository.Instance.SaveDuMeasurement(_measurement);
                MessagingCenter
                    .Send<DuMeasurementDonePage, DuMeasurement>(
                    this, "Refresh measurement", _measurement);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OnDisappearing");
                throw;
            }
        }
    }
}