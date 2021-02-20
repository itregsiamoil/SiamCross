using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        private readonly ScannerViewModel _vm;

        public ScanningTab(IBluetoothScanner scanner)
        {
            _vm = new ScannerViewModel(scanner);
            //_vm.Scanner.ScanStarted += () => { RView.IsRefreshing = true; };
            //_vm.Scanner.ScanStoped += () => { RView.IsRefreshing = false; };
            BindingContext = _vm;
            InitializeComponent();
        }
        public ScanningTab()
            : this(FactoryBtLe.GetCurent().GetScanner())
        {
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _vm.StopScan();
            base.OnDisappearing();
        }
    }

    public class BoundingTab : ScanningTab
    {
        public BoundingTab()
            : base(FactoryBt2.GetCurent().GetScanner())
        {

        }
    }
}