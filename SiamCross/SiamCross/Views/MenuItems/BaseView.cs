using Autofac;
using SiamCross.AppObjects;
using SiamCross.ViewModels;
using Xamarin.Forms;

namespace SiamCross.Views.MenuItems
{
    public class BaseView<T> : ContentView where T : IViewModel
    {
        public T ViewModel { get; }

        public BaseView()
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>();
            }
            BindingContext = ViewModel;
        }
    }
}
