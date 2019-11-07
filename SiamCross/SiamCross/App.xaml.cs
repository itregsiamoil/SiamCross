using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SiamCross.Views;
using SiamCross.ViewModels;
using MvvmCross;
using SiamCross.Services;
using MvvmCross.Plugin.Messenger;

namespace SiamCross
{
    public partial class App : Application
    {
        public static NavigationPage NavigationPage { get; private set; }
        public static RootPage RootPage;
        public static bool MenuIsPresented
        {
            get
            {
                return RootPage.IsPresented;
            }
            set
            {
                RootPage.IsPresented = value;
            }
        }

        private void CallMain()
        {
            var menuPage = new MenuPage() { Title = "Title" };
            NavigationPage = new NavigationPage(new Home());
            RootPage = new RootPage();
            RootPage.Master = menuPage;
            RootPage.Detail = NavigationPage;
            MainPage = RootPage;
        }

        public App()
        {
            InitializeComponent();

            // MainPage = new MainPage();
            var vm = new ScannerViewModel(DependencyService.Resolve<IScannedDevicesService>(),
                Mvx.IoCProvider.Resolve<IMvxMessenger>());
            MainPage = new ScannerPage(vm);
           // CallMain();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
