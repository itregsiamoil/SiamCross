using SiamCross.ViewModels;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public class BaseContentPage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is BasePageVM vm)
            {
                vm.EnableOrintationNotify = true;
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is BasePageVM vm)
            {
                vm.EnableOrintationNotify = false;
            }
        }
        /*
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (null == BindingContext)
                return;
            if (BindingContext is BasePageVM vm)
                vm.OrintationNotify();
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            if (BindingContext is BasePageVM vm)
            {
                vm.ChangeNotify(nameof(BasePageVM.IsLandscape));
                vm.ChangeNotify(nameof(BasePageVM.IsPortrait));
            }
        }
        */
    }
}
