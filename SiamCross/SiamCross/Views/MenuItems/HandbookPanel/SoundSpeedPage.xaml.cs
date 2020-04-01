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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundSpeedPage : ContentPage
    {
        public SoundSpeedPage()
        {
            var vm = new ViewModelWrap<SoundSpeedViewModel>();
            this.BindingContext = vm.ViewModel;
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
                var stack = App.NavigationPage.Navigation.NavigationStack;
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
            var dialog = AppContainer.Container.Resolve<IFileOpenDialog>();
           
            var path = await dialog.Show();

            if(string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var fileParcer = new SoundSpeedFileParcer();
            List<KeyValuePair<float, float>> newSoundTable;

            using (StreamReader reader = new StreamReader(path))
            {
                newSoundTable = fileParcer.TryToParce(reader.ReadToEnd());
            }

            if(newSoundTable == null)
            {
                await DisplayAlert(
                    Resource.Attention, 
                    Resource.WrongFormatOrContent,
                    Resource.Ok);
                return;
            }

            var newSpeedSoundModel = new SoundSpeedModel(0, "", newSoundTable);

            await App.NavigationPage.Navigation.PushAsync(
                            new SoundSpeedViewPage(newSpeedSoundModel));
        }
    }
}