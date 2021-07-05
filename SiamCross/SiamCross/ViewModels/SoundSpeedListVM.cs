using NLog;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views.MenuItems.HandbookPanel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SoundSpeedListVM : BaseDirectoryPageVM
    {
        private static readonly Logger _logger = DependencyService.Get<ILogManager>().GetLog();
        public SoundSpeedListVM()
            : base(Resource.SoundSpeed)
        {
        }
        public override Task InitAsync(CancellationToken ct = default)
        {
            Repo.SoundSpeedDir.Models.CollectionChanged += Models_CollectionChanged;
            Models_CollectionChanged(null, null);
            return Task.CompletedTask;
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Repo.SoundSpeedDir.Models.CollectionChanged -= Models_CollectionChanged;
        }
        private void Models_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                SortedSet<BaseDirectoryItem> list;
                switch (_SortOrder)
                {
                    default:
                    case SortOrder.AscId:
                        list = new SortedSet<BaseDirectoryItem>(new AscIdComparer());
                        break;
                    case SortOrder.DescId:
                        list = new SortedSet<BaseDirectoryItem>(new DescIdComparer());
                        break;
                    case SortOrder.AscTitle:
                        list = new SortedSet<BaseDirectoryItem>(new AscTitleComparer());
                        break;
                    case SortOrder.DescTitle:
                        list = new SortedSet<BaseDirectoryItem>(new DescTitleComparer());
                        break;
                }
                foreach (var item in Repo.SoundSpeedDir.Models)
                    list.Add(new BaseDirectoryItem((uint)item.Code, item.Name));
                Items.ReplaceRange(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
            }
        }
        protected override async Task OnCmdEdit(object item)
        {
            try
            {
                if (IsMultiselectMode
                    || !(item is BaseDirectoryItem itemModel)
                    || !Repo.SoundSpeedDir.DictById.TryGetValue(itemModel.Key, out SoundSpeedModel ssModel))
                    return;
                await App.NavigationPage.Navigation.PushAsync(new EditSoundSpeedPage(ssModel));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
                _logger.Error(ex, "Edit command handler" + "\n");
            }
        }
        protected override async Task OnCmdDel()
        {
            try
            {
                var list = new List<uint>(SelectedItems.Count);
                foreach (var viewItem in SelectedItems)
                    if (viewItem is BaseDirectoryItem item)
                        list.Add(item.Key);
                _ = await Repo.SoundSpeedDir.DeleteAsync(list);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
                _logger.Error(ex, "OnCmdDel command handler" + "\n");
            }
        }
        protected override async Task OnCmdAdd()
        {
            try
            {
                await App.NavigationPage.Navigation.PushAsync(new EditSoundSpeedPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
                _logger.Error(ex, "OpenAddFieldsPage command handler" + "\n");
            }
        }
    }
}
