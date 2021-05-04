using SiamCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class MeasurementsPage : BaseContentPage
    {
        private readonly MeasurementsVMService _vm = MeasurementsVMService.Instance;
        public MeasurementsPage()
        {
            InitializeComponent();
        }
        private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IReadOnlyList<object> previous = e.PreviousSelection;
            IReadOnlyList<object> current = e.CurrentSelection;
            _vm.UpdateSelectedItems(previous, current);
        }

        private void OnSelectItem(object sender, SelectionChangedEventArgs e)
        {
            _vm.OnItemTapped(e.PreviousSelection.FirstOrDefault() as MeasurementView);
        }

        private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!(sender is CheckBox chk))
                return;
            if (!(chk.BindingContext is MeasurementView meas))
                return;

            _vm.UpdateSelect(meas, e.Value);
        }

        protected override bool OnBackButtonPressed()
        {
            return _vm.OnBackButton();
        }
        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            ((ListView)sender).SelectedItem = null;
            _vm.OnItemTapped(e.Item as MeasurementView);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.DoOnDisappearing();
            _vm.OnBackButton();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.ReloadMeasurementsFromDb().ConfigureAwait(false);
        }
    }
}