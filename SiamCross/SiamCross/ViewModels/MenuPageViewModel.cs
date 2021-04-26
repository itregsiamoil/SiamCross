using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.Views.MenuItems;
using SiamCross.Views.MenuItems.HandbookPanel;
using SiamCross.Views.MenuItems.SearchPanel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class MenuPageViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private MenuPageItem _selectedItem;

        public class MenuPageItem
        {
            public string Title { get; set; }

            public ICommand Command { get; set; }
        }

        public List<MenuPageItem> MenuItems { get; private set; }

        public ICommand GoControlPanel { get; set; }
        public ICommand GoSearchPanel { get; set; }
        public ICommand GoMeasuringPanel { get; set; }
        public ICommand GoDirectoryPanel { get; set; }
        public ICommand GoSettingsPanel { get; set; }
        public ICommand GoAboutPanel { get; set; }

        public MenuPageItem SelectedItem
        {
            get => _selectedItem;
            set => _selectedItem = value;
        }
        public MenuPageViewModel()
        {
            GoControlPanel = CreateAsyncCommand(GoHome);
            GoSearchPanel = CreateAsyncCommand(GoSearch);
            GoMeasuringPanel = CreateAsyncCommand(GoMeasuring);
            GoDirectoryPanel = CreateAsyncCommand(GoHandbook);
            GoSettingsPanel = CreateAsyncCommand(GoSettings);
            GoAboutPanel = CreateAsyncCommand(GoAbout);

            MenuItems = new List<MenuPageItem>
            {
                new MenuPageItem()
                {
                    Title = Resource.ControlPanelTitle,
                    Command = GoControlPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.SearchTitle,
                    Command = GoSearchPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.MeasurementsTitle,
                    Command = GoMeasuringPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.HandbookTitle,
                    Command = GoDirectoryPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.SettingsTitle,
                    Command = GoSettingsPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.AboutTitle,
                    Command = GoAboutPanel
                }
            };
        }
        AsyncCommand CreateAsyncCommand(Func<Task> t)
        {
            return new AsyncCommand(t, () => App.NavigationPage.IsBusy, null, false, false);
        }
        private async Task GoHome()
        {
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PopToRootAsync();
        }
        private async Task GoSearch()
        {
            await App.NavigationPage.Navigation.PopToRootAsync(false);
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new SearchPanelPage());
        }
        private async Task GoMeasuring()
        {
            await App.NavigationPage.Navigation.PopToRootAsync(false);
            App.MenuIsPresented = false;
            await Services.PageNavigator.ShowPageAsync(MeasurementsVMService.Instance);
        }
        private async Task GoHandbook()
        {
            await App.NavigationPage.Navigation.PopToRootAsync(false);
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new HandbookPage());
        }
        private async Task GoSettings()
        {
            await App.NavigationPage.Navigation.PopToRootAsync(false);
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new SettingsPanelPage());
        }
        private async Task GoAbout()
        {
            await App.NavigationPage.Navigation.PopToRootAsync(false);
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new AboutPanelPage());
        }

        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
    }
}
