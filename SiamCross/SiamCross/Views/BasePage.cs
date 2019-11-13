using Autofac;
using SiamCross.AppObjects;
using SiamCross.ViewModels;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public class BasePage<T> : ContentPage where T:IViewModel
    {
        public T ViewModel { get; }

        public BasePage()
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>();
            }
            BindingContext = ViewModel;
        }
    }
}
