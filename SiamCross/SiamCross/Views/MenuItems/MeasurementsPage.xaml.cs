using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsPage : ContentPage
    {
        private readonly MeasurementsViewModel _vm;
        public MeasurementsPage()
        {
            _vm = new ViewModelWrap<MeasurementsViewModel>().ViewModel;
            BindingContext = _vm;
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                _vm.PushPage(e.Item as MeasurementView);
            }
        }
    }
}