using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsPage : ContentPage
    {
        private MeasurementsViewModel _vm;
        public MeasurementsPage()
        {
            _vm = new ViewModel<MeasurementsViewModel>().GetViewModel;
            this.BindingContext = _vm;
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