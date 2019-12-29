using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.Views.MenuItems;
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
    public partial class Ddin2MeasurementDonePage : ContentPage
    {
        private Ddin2Measurement _measurement;
        public Ddin2MeasurementDonePage(Ddin2Measurement measurement)
        {
            _measurement = measurement;
            var vm = new ViewModel<Ddin2MeasurementDoneViewModel>(measurement);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            DataRepository.Instance.SaveDdin2Item(_measurement);
            NavigationHelper.RemovePage(typeof(MeasurementsPage));
            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
            await App.NavigationPage.Navigation.PushAsync(
                  new MeasurementsPage(), true);
        }
    }
}