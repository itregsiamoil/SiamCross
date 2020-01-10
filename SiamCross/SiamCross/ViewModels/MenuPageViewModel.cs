using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using SiamCross.Models;
using Xamarin.Forms;
using SiamCross.Views;
using SiamCross.Views.MenuItems;

namespace SiamCross.ViewModels
{
    public class MenuPageViewModel
    {
        public ICommand GoControlPanel { get; set; }
        public ICommand GoSearchPanel { get; set; }
        public ICommand GoMeasuringPanel { get; set; }
        public ICommand GoDirectoryPanel { get; set; }
        public ICommand GoSettingsPanel { get; set; }
        public ICommand GoAboutPanel { get; set; }
        public MenuPageViewModel()
        {
            GoControlPanel = new Command(GoHome);
            GoSearchPanel = new Command(GoSearch);
            GoMeasuringPanel = new Command(GoMeasuring);
            GoDirectoryPanel = new Command(GoDirectory);
            GoSettingsPanel = new Command(GoSettings);
            GoAboutPanel = new Command(GoAbout);
        }

        void GoHome(object obj)
        {
            App.NavigationPage.Navigation.PopToRootAsync();
            App.MenuIsPresented = false;
        }

        void GoSearch(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new SearchPanelPage()); 
            App.MenuIsPresented = false;
        }

        void GoMeasuring(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new MeasurementsPage());
            App.MenuIsPresented = false;
        }

        void GoDirectory(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new DirectoryPanelPage());
            App.MenuIsPresented = false;
        }

        void GoSettings(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new SettingsPanelPage());
            App.MenuIsPresented = false;
        }

        void GoAbout(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new AboutPanelPage());
            App.MenuIsPresented = false;
        }
    }
}
