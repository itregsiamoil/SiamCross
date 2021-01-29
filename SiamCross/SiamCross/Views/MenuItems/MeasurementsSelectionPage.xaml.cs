using SiamCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsSelectionPage : ContentPage
    {
        private readonly MeasurementsSelectionViewModel _vm = null;
        public MeasurementsSelectionPage()
        {
            _vm = new MeasurementsSelectionViewModel();
            base.BindingContext = _vm;
            InitializeComponent();
        }
        public MeasurementsSelectionPage(MeasurementsSelectionViewModel vm)
        {
            _vm = vm;
            base.BindingContext = _vm;
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
            CheckBox chk = sender as CheckBox;
            if (null == chk)
                return;
            MeasurementView meas = chk.BindingContext as MeasurementView;
            if (null == meas)
                return;
            
            List<object> previous = new List<object>(_vm.SelectedMeasurements);
            if (e.Value)
            {
                if (!_vm.SelectedMeasurements.Contains(meas))
                    _vm.SelectedMeasurements.Add(meas);
            }
            else
                _vm.SelectedMeasurements.Remove(meas);
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

    }
}