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
    public partial class Ddim2MeasurementDonePage : ContentPage
    {
        private Ddim2Measurement _measurement;
        public Ddim2MeasurementDonePage(Ddim2Measurement measurement)
        {
            _measurement = measurement;
            var vm = new ViewModel<Ddim2MeasurementDoneViewModel>(measurement);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            DataRepository.Instance.SaveDdim2Item(_measurement);
            NavigationHelper.RemovePage(typeof(MeasurementsPage));
            //this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

            await App.NavigationPage.Navigation.PushAsync(
                new MeasurementsPage(), true);
        }
    }
}