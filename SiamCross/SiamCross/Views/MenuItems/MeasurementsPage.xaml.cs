using SiamCross.ViewModels;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

/*
namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsPage : ContentPage
    {
        private readonly MeasurementsSelectionViewModel _vm;
        public MeasurementsPage()
        {
            _vm = new ViewModelWrap<MeasurementsSelectionViewModel>().ViewModel;
            BindingContext = _vm;
            InitializeComponent();
        }

        private void OnSelectItem(object sender, SelectionChangedEventArgs e)
        {
            _vm.PushPage(e.CurrentSelection.FirstOrDefault() as MeasurementView);
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
*/