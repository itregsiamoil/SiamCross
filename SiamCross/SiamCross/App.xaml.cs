﻿using SiamCross.AppObjects;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Helpers;
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
        private async void MainInit()
        {
            var initNavigator = PageNavigator.Init();
            var initDbService = DbService.Instance.Init();
            var tasks = new List<Task>(5);
            tasks.Add(initNavigator);
            tasks.Add(initDbService);
            await Task.WhenAll(tasks).ConfigureAwait(false);
            tasks.Clear();
            var initRepo = Repo.InitAsync();
            var initSensorService = SensorService.Instance.InitinalizeAsync();
            tasks.Add(initRepo);
            tasks.Add(initSensorService);
            await Task.WhenAll(tasks).ConfigureAwait(false);
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
            LocalizationResourceManager.Current.PropertyChanged += Current_PropertyChanged;
            LocalizationResourceManager.Current.Init(Resource.ResourceManager, TranslateCfg.Current);

            InitializeComponent();
            setup.RegisterDependencies();

            CallMain();
            MainInit();
        }

        private async void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Resource.Culture = LocalizationResourceManager.Current.CurrentCulture;
            if (null != RootPage && null != RootPage.Flyout)
                RootPage.Flyout.BindingContext = new MenuPageViewModel();
            if (null != NavigationPage)
            {
                //await PageNavigator.Init();
                await SensorService.Instance.InitinalizeAsync();
                await NavigationPage.PopToRootAsync();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //DependencyService.Get<INotificationManager>().SendNotification("testTitle", "test message");
        }
        protected override void OnSleep()
        {
            //await App.Navigation.PopToRootAsync();
            base.OnSleep();
            // Handle when your app sleeps
            System.Diagnostics.Debug.WriteLine($"OnSleep");
        }
        protected override void OnResume()
        {
            base.OnResume();
            // Handle when your app resumes
        }
    }
}
