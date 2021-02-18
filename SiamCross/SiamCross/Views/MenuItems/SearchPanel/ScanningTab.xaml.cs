using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        private readonly ScannerViewModel _vm;
        public ScanningTab()
        {
            InitializeComponent();
            _vm = new ViewModelWrap<ScannerViewModel>().ViewModel;
            _vm.Scanner.ScanStarted += () => { RView.IsRefreshing = true; };
            _vm.Scanner.ScanStoped += () => { RView.IsRefreshing = false; };
            BindingContext = _vm;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.AppendBonded();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.StopScan();
        }
    }
}