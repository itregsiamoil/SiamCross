using SiamCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class MeasurementsPage : BaseContentPage
    {
        private Task InitTask;
        private CancellationTokenSource Cts;
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
            _vm.UpdateSelectTitle();
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
            IsBusy = true;
            base.OnDisappearing();
            InitTask = Task.Run(() => VmDeinitAsync(Cts.Token));
        }
        protected override void OnAppearing()
        {
            IsBusy = true;
            if (null != Cts)
            {
                if (Cts.IsCancellationRequested)
                {
                    Cts.Dispose();
                    Cts = new CancellationTokenSource();
                }
            }
            else
                Cts = new CancellationTokenSource();
            InitTask = Task.Run(() => VmInitAsync(Cts.Token));
            base.OnAppearing();
        }
        protected async Task VmInitAsync(CancellationToken ct)
        {
            await _vm.ReloadMeasurementsFromDb();
            IsBusy = false;
            /*
            if (MainThread.IsMainThread)
            {
                IsBusy = false;
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => { IsBusy = false; });
            }
            */
        }
        protected async Task VmDeinitAsync(CancellationToken ct)
        {
            if (null == _vm)
                return;
            if (null != InitTask && !InitTask.IsCompleted)
            {
                Cts?.Cancel();
                await InitTask;
            }
            _vm.DoOnDisappearing();
            _vm.OnBackButton();
            IsBusy = false;
        }
    }
}