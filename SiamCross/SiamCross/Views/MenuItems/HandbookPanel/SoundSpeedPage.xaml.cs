using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.Services.UserOutput;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundSpeedPage : ContentPage
    {
        public SoundSpeedPage()
        {
            ViewModelWrap<SoundSpeedViewModel> vm = new ViewModelWrap<SoundSpeedViewModel>();
            BindingContext = vm.ViewModel;
            InitializeComponent();
        }

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            OpenSoundSpeedViewPage(e);
        }

        private void OpenSoundSpeedViewPage(ItemTappedEventArgs itemTapped)
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(SoundSpeedViewPage))
                    {
                        App.NavigationPage.Navigation.PushAsync(
                            new SoundSpeedViewPage((SoundSpeedModel)itemTapped.Item));
                    }
                }
                else
                {
                    App.NavigationPage.Navigation.PushAsync(
                        new SoundSpeedViewPage((SoundSpeedModel)itemTapped.Item));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OpenSoundSpeedViewPage command handler" + "\n");
                throw;
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            IFileOpenDialog dialog = AppContainer.Container.Resolve<IFileOpenDialog>();

            string path = await dialog.Show();

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            SoundSpeedFileParcer fileParcer = new SoundSpeedFileParcer();
            List<KeyValuePair<float, float>> newSoundTable;

            using (StreamReader reader = new StreamReader(path))
            {
                newSoundTable = fileParcer.TryToParce(reader.ReadToEnd());
            }

            if (newSoundTable == null)
            {
                await DisplayAlert(
                    Resource.Attention,
                    Resource.WrongFormatOrContent,
                    Resource.Ok);
                return;
            }

            SoundSpeedModel newSpeedSoundModel = new SoundSpeedModel(0, "", newSoundTable);

            await App.NavigationPage.Navigation.PushAsync(
                            new SoundSpeedViewPage(newSpeedSoundModel));
        }
    }
}