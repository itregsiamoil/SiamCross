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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        readonly DelegateWeakEventManager _propertyChangedEventManager = new DelegateWeakEventManager();

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        protected void SetProperty<T>(ref T backingStore, in T value, in System.Action onChanged = null, [CallerMemberName] in string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        protected void OnPropertyChanged([CallerMemberName] in string propertyName = "")
        {
            _propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
        }
    }

    public class SoundSpeedListVM : BaseViewModel
    {
        /*
        readonly WeakEventManager<NotifyCollectionChangedEventArgs> _EventManager
            = new WeakEventManager<NotifyCollectionChangedEventArgs>();
        
        public event EventHandler<NotifyCollectionChangedEventArgs> GetNotifyCollectionChanged
        {
            add => _EventManager.AddEventHandler(value);
            remove => _EventManager.RemoveEventHandler(value);
        }
        //void OnGetLatestReleaseFailed(string message) => _getLatestReleaseFailedEventManager.RaiseEvent(this, message, nameof(GetLatestReleaseFailed));
        */

        public ICommand CmdAdd { get; }
        public ICommand CmdDel { get; }
        public ICommand CmdEdit { get; }
        public ObservableCollection<SoundSpeedModel> SoundSpeedList { get; set; }
        object _SelectedItem;
        public object SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public SoundSpeedListVM()
        {
            SoundSpeedList = new ObservableCollection<SoundSpeedModel>();

            CmdAdd = new AsyncCommand(AddSound
                , (Func<object, bool>)null, null, false, false);
            CmdDel = new AsyncCommand(DelSound
                , (Func<object, bool>)null, null, false, false);
            CmdEdit = new AsyncCommand<object>(EditSound
                , (Func<object, bool>)null, null, false, false);
            InitAsync();
        }
        public Task InitAsync(CancellationToken ct = default)
        {
            //Repo.SoundSpeedDir.Models.CollectionChanged += Models_CollectionChanged;
            //Repo.SoundSpeedDir.Models.CollectionChanged += Models_CollectionChanged;

            //Repo.SoundSpeedDir._EventManager += Models_CollectionChanged;


            //Repo.SoundSpeedDir._getCollectionChangedEventManager
            //    .AddEventHandler(Models_CollectionChanged, "Event");

            Update();
            return Task.CompletedTask;
        }
        public void Unsubscribe()
        {
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
                SoundSpeedList.Clear();
                foreach (SoundSpeedModel soundItem in Repo.SoundSpeedDir.Models)
                {
                    SoundSpeedList.Add(soundItem);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
                throw;
            }
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

                    if (SelectedItem is SoundSpeedModel ssModel)
                    {
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
