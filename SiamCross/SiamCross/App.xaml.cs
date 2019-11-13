﻿using Autofac;
using Autofac.Core;
using SiamCross.AppObjects;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
            var menuPage = new MenuPage() { Title = "SiamServiceMenu" };
            NavigationPage = new NavigationPage(new ControlPanelPage());
            RootPage = new RootPage();
            RootPage.Master = menuPage;
            RootPage.Detail = NavigationPage;
            MainPage = RootPage;
        }

        public App(AppSetup setup)
        {
            InitializeComponent();
            AppContainer.Container = setup.CreateContainer();
            // MainPage = new MainPage();
            CallMain();
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
