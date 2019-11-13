using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SiamCross.Views;
using SiamCross.ViewModels;
using SiamCross.Services;

using SiamCross.Views.MenuItems;
using Xamarin.Forms.Internals;
using Autofac;
using System.Collections.Generic;
using Autofac.Core;

namespace SiamCross
{
    public partial class App : Application
    {
        private static IContainer _container;
        private static readonly ContainerBuilder _builder = new ContainerBuilder();
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

        public App()
        {
            InitializeComponent();

            // MainPage = new MainPage();
            DependencyResolver.ResolveUsing(type => _container.IsRegistered(type) ? _container.Resolve(type) : null);
            CallMain();
        }

        public static void RegisterType<T>() where T : class
        {
            _builder.RegisterType<T>();
        }

        public static void RegisterType<TInterface, T>() where TInterface : class where T : class, TInterface
        {
            _builder.RegisterType<T>().As<TInterface>();
        }

        public static void RegisterTypeWithParameters<T>(Type param1Type, object param1Value, Type param2Type, string param2Name) where T : class
        {
            _builder.RegisterType<T>()
                   .WithParameters(new List<Parameter>()
            {
                new TypedParameter(param1Type, param1Value),
                new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == param2Type && pi.Name == param2Name,
                    (pi, ctx) => ctx.Resolve(param2Type))
            });
        }

        public static void RegisterTypeWithParameters<TInterface, T>(Type param1Type, object param1Value, Type param2Type, string param2Name) where TInterface : class where T : class, TInterface
        {
            _builder.RegisterType<T>()
                   .WithParameters(new List<Parameter>()
            {
                new TypedParameter(param1Type, param1Value),
                new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == param2Type && pi.Name == param2Name,
                    (pi, ctx) => ctx.Resolve(param2Type))
            }).As<TInterface>();
        }

        public static void BuildContainer()
        {
            _container = _builder.Build();
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
