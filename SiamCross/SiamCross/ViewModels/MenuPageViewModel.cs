using NLog;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.Views.MenuItems;
using SiamCross.Views.MenuItems.SearchPanel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MenuPageItem
    {
        public string Title { get; set; }
        public ICommand Command { get; set; }
    }

    public class MenuPageViewModel
    {
        private static readonly Logger _logger = DependencyService.Get<ILogManager>().GetLog();

        private MenuPageItem _selectedItem;


        public List<MenuPageItem> MenuItems { get; private set; }

        public ICommand GoControlPanel { get; set; }
        public ICommand GoSearchPanel { get; set; }
        public ICommand GoMeasuringPanel { get; set; }
        public ICommand GoMailSettings { get; set; }
        public ICommand GoFieldDir { get; set; }
        public ICommand GoSoundSpeedDir { get; }
        public ICommand GoAboutPanel { get; set; }
        public ICommand GoLanguagePanel { get; set; }

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
            GoMailSettings = CreateAsyncCommand(GoMail);
            GoFieldDir = CreateAsyncCommand(GoField);
            GoSoundSpeedDir = CreateAsyncCommand(GoSoundSpeed);
            GoAboutPanel = CreateAsyncCommand(GoAbout);
            GoLanguagePanel = CreateAsyncCommand(GoLanguage);

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
                    Title = Resource.Surveys,
                    Command = GoMeasuringPanel
                },
                new MenuPageItem()
                {
                    Title = Resource.EmailSettings,
                    Command = GoMailSettings
                },
                new MenuPageItem()
                {
                    Title = Resource.Fields,
                    Command = GoFieldDir
                },
                new MenuPageItem()
                {
                    Title = Resource.SoundSpeed,
                    Command = GoSoundSpeedDir
                },
                new MenuPageItem()
                {
                    Title = Resource.AboutTitle,
                    Command = GoAboutPanel
                },
                new MenuPageItem()
                {
                    Title = "\u2691 "+Resource.Language,
                    Command = GoLanguagePanel
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
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new SearchPanelPage());
        }
        private async Task GoMeasuring()
        {
            App.MenuIsPresented = false;
            await Services.PageNavigator.ShowPageAsync(MeasurementsVMService.Instance);
        }
        private async Task GoMail()
        {
            App.MenuIsPresented = false;
            await Services.PageNavigator.ShowPageAsync(new MailSettingsVM());
        }
        private async Task GoField()
        {
            App.MenuIsPresented = false;
            await Services.PageNavigator.ShowPageAsync(new FieldsDirVM());
        }
        private async Task GoSoundSpeed()
        {
            App.MenuIsPresented = false;
            await Services.PageNavigator.ShowPageAsync(new SoundSpeedListVM());
        }

        private async Task GoAbout()
        {
            App.MenuIsPresented = false;
            await App.NavigationPage.Navigation.PushAsync(new AboutPanelPage());
        }

        private async Task GoLanguage()
        {
            App.MenuIsPresented = false;
            string action = await Application.Current.MainPage
                .DisplayActionSheet(Resource.SelectLanguage
                , Resource.Cancel, null, TranslateCfg.SupportedLanguages);
            if (string.IsNullOrEmpty(action) || action == Resource.Cancel)
                return;
            string lang = "Auto";

            if (TranslateCfg.SupportedLanguages[1] == action)
                lang = "ru";
            else if (TranslateCfg.SupportedLanguages[2] == action)
                lang = "en";
            TranslateCfg.SetCulture(lang);
            await App.NavigationPage.DisplayToastAsync(Resource.ChangingLanguage, 5000);
        }

        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();


    }
}
