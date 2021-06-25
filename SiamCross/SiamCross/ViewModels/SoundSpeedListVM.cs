using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Services.StdDialog;
using SiamCross.Views.MenuItems.HandbookPanel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SoundSpeedListVM : BasePageVM
    {
        public bool IsMultiselectMode => _SelectionMode == SelectionMode.Multiple;


        SelectionMode _SelectionMode = SelectionMode.None;
        public SelectionMode SelectionMode
        {
            get => _SelectionMode;
            set
            {
                SetProperty(ref _SelectionMode, value);
                ChangeNotify(nameof(IsMultiselectMode));
            }
        }
        public ICommand CmdLongPress { get; }
        public ICommand CmdAdd { get; }
        public ICommand CmdDel { get; }
        public ICommand CmdEdit { get; }
        public ObservableRangeCollection<SoundSpeedModel> SoundSpeedList { get; }
        public ObservableRangeCollection<object> SelectedItems { get; }
        public object SelectedItem { get; set; }

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public SoundSpeedListVM()
        {
            SoundSpeedList = new ObservableRangeCollection<SoundSpeedModel>();
            SelectedItems = new ObservableRangeCollection<object>();

            CmdLongPress = new Command(OnLongPress);

            CmdAdd = new AsyncCommand(AddSound
                , (Func<object, bool>)null, null, false, false);
            CmdDel = new AsyncCommand(DelSound
                , (Func<object, bool>)null, null, false, false);
            CmdEdit = new AsyncCommand<object>(EditSound
                , (Func<object, bool>)null, null, false, false);

        }
        public Task InitAsync(CancellationToken ct = default)
        {
            Repo.SoundSpeedDir.Models.CollectionChanged += Models_CollectionChanged;
            Update();
            return Task.CompletedTask;
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Repo.SoundSpeedDir.Models.CollectionChanged -= Models_CollectionChanged;
        }

        private void Models_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            try
            {
                SoundSpeedList.ReplaceRange(Repo.SoundSpeedDir.Models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
            }
        }

        private void OnLongPress()
        {
            SelectionMode = SelectionMode.Multiple;
        }
        public bool OnBackButton()
        {
            if (IsMultiselectMode)
            {
                SelectedItems.Clear();
                SelectionMode = SelectionMode.None;
                return true;
            }
            return false;
        }


        private async Task AddSound()
        {
            string path = await StdDialogService.Instance.ShowOpenDialog();

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            List<KeyValuePair<float, float>> newSoundTable;

            using (StreamReader reader = new StreamReader(path))
            {
                newSoundTable = SoundSpeedFileParcer.TryToParce(reader.ReadToEnd());
            }

            if (newSoundTable == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    Resource.Attention,
                    Resource.WrongFormatOrContent,
                    Resource.Ok);
                return;
            }

            SoundSpeedModel newSpeedSoundModel = new SoundSpeedModel(0, "", newSoundTable);

            await App.NavigationPage.Navigation.PushAsync(
                            new SoundSpeedViewPage(newSpeedSoundModel));
        }
        private async Task DelSound()
        {
            bool result = await Application.Current.MainPage.DisplayAlert(
                  Resource.Attention,
                  Resource.DeleteQuestion,
                  Resource.YesButton,
                  Resource.NotButton);
            if (result)
            {
                try
                {
                    foreach (var item in SelectedItems)
                    {
                        if (item is SoundSpeedModel ssModel)
                            _ = await Repo.SoundSpeedDir.DeleteAsync((uint)ssModel.Code);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
                }

            }
        }
        private async Task EditSound(object item)
        {
            try
            {
                if (IsMultiselectMode)
                    return;

                SelectedItem = item;
                if (!(SelectedItem is SoundSpeedModel soundModel))
                    return;
                var idx = SoundSpeedList.IndexOf(soundModel);
                await App.NavigationPage.Navigation.PushAsync(
                        new SoundSpeedViewPage(soundModel));
                SoundSpeedList.Insert(idx, soundModel);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "EditSound command handler" + "\n");
            }
        }

    }
}
