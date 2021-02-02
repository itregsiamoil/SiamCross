﻿using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsPageService : ContentPage
    {
        private static readonly Lazy<MeasurementsPageService> _instance =
            new Lazy<MeasurementsPageService>(() => new MeasurementsPageService());
        public static MeasurementsPageService Instance => _instance.Value;

        private readonly MeasurementsVM _vm = null;
        private MeasurementsPageService()
        {
            _vm = new MeasurementsVM();
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

        protected override void OnAppearing()
        {
            _vm.ReloadMeasurementsFromDb();
        }
    }
}