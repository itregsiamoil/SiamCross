using SiamCross.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsSelectionPage : ContentPage
    {
        private readonly MeasurementsSelectionViewModel _vm = null;
        public MeasurementsSelectionPage(MeasurementsSelectionViewModel vm)
        {
            _vm = vm;
            base.BindingContext = _vm;
            InitializeComponent();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            _vm.SaveMeasurements(sender);
        }

        private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Collections.Generic.IReadOnlyList<object> previous = e.PreviousSelection;
            System.Collections.Generic.IReadOnlyList<object> current = e.CurrentSelection;
            _vm.UpdateSelectedItems(previous, current);
        }

        private void OnSelectItem(object sender, SelectionChangedEventArgs e)
        {
            _vm.UpdateSelectedItem(e.PreviousSelection.FirstOrDefault() as MeasurementView);
        }

    }
}