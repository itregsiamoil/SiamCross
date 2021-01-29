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
using System.Windows.Input;
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
            GoControlPanel = new Command(GoHome);
            GoSearchPanel = new Command(GoSearch);
            GoMeasuringPanel = new Command(GoMeasuring);
            GoDirectoryPanel = new Command(GoHandbook);
            GoSettingsPanel = new Command(GoSettings);
            GoAboutPanel = new Command(GoAbout);

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

        private void GoHome(object obj)
        {
            App.NavigationPage.Navigation.PopToRootAsync();
            App.MenuIsPresented = false;
        }
        /// <summary>
        /// Проверяет на соответствие параметр и
        /// тип последней страницы в стеке навигации.
        /// Если страничка уже открыта, возвращает false
        /// </summary>
        /// <param name="type">Тип проверяемой страницы</param>
        /// <returns></returns>
        private bool CanOpenPage(Type type)
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
                if (stack[stack.Count - 1].GetType() != type)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanOpenPage method" + "\n");
                throw;
            }
        }

        private void GoSearch(object obj)
        {
            if (CanOpenPage(typeof(SearchPanelPage)))
            {
                App.NavigationPage.Navigation.PushAsync(new SearchPanelPage());
                App.MenuIsPresented = false;
            }
        }

        private void GoMeasuring(object obj)
        {
            if (CanOpenPage(typeof(MeasurementsSelectionPage)))
            {
                App.NavigationPage.Navigation.PushAsync(new MeasurementsSelectionPage());
                App.MenuIsPresented = false;
            }
        }

        private void GoHandbook(object obj)
        {
            if (CanOpenPage(typeof(DirectoryPage)))
            {
                App.NavigationPage.Navigation.PushAsync(new HandbookPage());
                App.MenuIsPresented = false;
            }
        }

        private void GoSettings(object obj)
        {
            if (CanOpenPage(typeof(SettingsPanelPage)))
            {
                App.NavigationPage.Navigation.PushAsync(new SettingsPanelPage());
                App.MenuIsPresented = false;
            }
        }

        private void GoAbout(object obj)
        {
            if (CanOpenPage(typeof(AboutPanelPage)))
            {
                App.NavigationPage.Navigation.PushAsync(new AboutPanelPage());
                App.MenuIsPresented = false;
            }
        }

        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
    }
}
