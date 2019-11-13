using Autofac;
using SiamCross.AppObjects;
using SiamCross.ViewModels;

namespace SiamCross.Views
{
    public class ViewModel<T> where T:IViewModel
    {
        public T GetViewModel { get; }

        public ViewModel()
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>();
            }
        }
    }
}
