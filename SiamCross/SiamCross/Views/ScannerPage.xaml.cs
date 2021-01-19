using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScannerPage : ContentPage
    {
        public ScannerViewModel ScannerVM { get; private set; }
        public ScannerPage(ScannerViewModel vm)
        {
            InitializeComponent();
            ScannerVM = vm;
            BindingContext = ScannerVM;
        }
    }
}