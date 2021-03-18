using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PositionEditView : ContentPage
    {
        public PositionEditView(PositionInfoVM vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}