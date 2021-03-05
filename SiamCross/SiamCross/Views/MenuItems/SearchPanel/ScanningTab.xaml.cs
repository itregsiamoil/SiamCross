using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningView : ContentPage
    {
        private readonly ScannerViewModel _ViewModel;
        public ScanningView(ScannerViewModel viewModel)
        {
            _ViewModel = viewModel;
            //_vm.Scanner.ScanStarted += () => { RView.IsRefreshing = true; };
            //_vm.Scanner.ScanStoped += () => { RView.IsRefreshing = false; };
            BindingContext = viewModel;
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _ViewModel.StopScan();
            base.OnDisappearing();
        }
    }

    public class ScanningTab : ScanningView
    {
        private static readonly ScannerViewModel _vm
            = new ScannerViewModel(FactoryBtLe.GetCurent().GetScanner());
        public ScanningTab()
            : base(_vm)
        {
        }

    }


    public class BoundingTab : ScanningView
    {
        private static readonly ScannerViewModel _vm
            = new ScannerViewModel(FactoryBt2.GetCurent().GetScanner());
        public BoundingTab()
            : base(_vm)
        {

        }
    }
}