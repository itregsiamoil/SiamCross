using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross
{
    [Preserve(AllMembers = true)]
    public partial class App : Application
    {
        public static NavigationPage NavigationPage { get; private set; }
        public static RootPage RootPage;
        public static INavigation Navigation { get; private set; }
        public static bool MenuIsPresented
        {
            get => RootPage.IsPresented;
            set => RootPage.IsPresented = value;
        }

        private void CallMain()
        {
            RootPage = new RootPage();

            MenuPage menuPage = new MenuPage() { Title = "SiamServiceMenu" };
            NavigationPage = new NavigationPage(new ControlPanelPage());

            RootPage.Flyout = menuPage;
            RootPage.Detail = NavigationPage;

            MainPage = RootPage;
            App.Navigation = NavigationPage.Navigation;
        }

        public App(AppSetup setup)
        {
            InitializeComponent();
            AppContainer.Container = setup.CreateContainer();

            if (Device.RuntimePlatform == Device.Android)
            {
                Resource.Culture = DependencyService.Get<ILocalize>()
                                    .GetCurrentCultureInfo();
            }

            CallMain();
        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            await Settings.Instance.Initialize();
        }

        protected override async void OnSleep()
        {
            await App.Navigation.PopToRootAsync();
            base.OnSleep();
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Handle when your app resumes
        }
    }
}
