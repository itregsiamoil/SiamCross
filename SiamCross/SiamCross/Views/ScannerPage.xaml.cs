using SiamCross.ViewModels;
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
    public partial class ScannerPage : ContentPage
    {
        public ScannerViewModel ScannerVM { get; private set; }
        public ScannerPage(ScannerViewModel vm)
        {
            InitializeComponent();
            ScannerVM = vm;
            this.BindingContext = ScannerVM;
        }
    }
}