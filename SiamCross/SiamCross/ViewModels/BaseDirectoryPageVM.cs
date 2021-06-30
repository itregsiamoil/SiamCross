using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class BaseDirectoryItem
    {
        public BaseDirectoryItem(uint key, string value)
        {
            Key = key;
            Value = value;
        }
        public uint Key { get; set; }
        public string Value { get; set; }
    }

    public abstract class BaseDirectoryPageVM : BasePageVM
    {
        internal class AscIdComparer : IComparer<BaseDirectoryItem>
        {
            public int Compare(BaseDirectoryItem x, BaseDirectoryItem y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
        internal class DescIdComparer : IComparer<BaseDirectoryItem>
        {
            public int Compare(BaseDirectoryItem x, BaseDirectoryItem y)
            {
                return y.Key.CompareTo(x.Key);
            }
        }
        internal class AscTitleComparer : IComparer<BaseDirectoryItem>
        {
            public int Compare(BaseDirectoryItem x, BaseDirectoryItem y)
            {
                return x.Value.CompareTo(y.Value);
            }
        }
        internal class DescTitleComparer : IComparer<BaseDirectoryItem>
        {
            public int Compare(BaseDirectoryItem x, BaseDirectoryItem y)
            {
                return y.Value.CompareTo(x.Value);
            }
        }
        protected enum SortOrder
        {
            AscId,
            DescId,
            AscTitle,
            DescTitle
        }
        protected SortOrder _SortOrder = SortOrder.AscId;

        private SelectionMode _SelectionMode = SelectionMode.None;
        public SelectionMode SelectionMode
        {
            get => _SelectionMode;
            set
            {
                SetProperty(ref _SelectionMode, value);
                ChangeNotify(nameof(IsMultiselectMode));
            }
        }
        public bool IsMultiselectMode => _SelectionMode == SelectionMode.Multiple;

        public string Title { get; }
        public ICommand CmdSort { get; }
        public ICommand CmdLongPress { get; }
        public ICommand CmdAdd { get; }
        public ICommand CmdDel { get; }
        public ICommand CmdEdit { get; }
        public ObservableRangeCollection<BaseDirectoryItem> Items { get; } //KeyValuePair<uint, string>
        public ObservableRangeCollection<object> SelectedItems { get; }

        public BaseDirectoryPageVM(string title)
        {
            Title = title;

            Items = new ObservableRangeCollection<BaseDirectoryItem>();
            SelectedItems = new ObservableRangeCollection<object>();

            CmdSort = new AsyncCommand(OnCmdSort
                , (Func<object, bool>)null, null, false, false);

            CmdLongPress = new Command(OnLongPress);

            CmdAdd = new AsyncCommand(OnCmdAdd
                , (Func<object, bool>)null, null, false, false);
            CmdDel = new AsyncCommand(OnCmdDel
                , (Func<object, bool>)null, null, false, false);
            CmdEdit = new AsyncCommand<object>(OnCmdEdit
                , (Func<object, bool>)null, null, false, false);
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
        protected async Task OnCmdSort()
        {
            var ordersVariant = new Dictionary<string, SortOrder>();
            ordersVariant.Add("По возрастанию идентификатора", SortOrder.AscId);
            ordersVariant.Add("По убыванию идентификатора", SortOrder.DescId);
            ordersVariant.Add("По возрастанию имени", SortOrder.AscTitle);
            ordersVariant.Add("По убыванию имени", SortOrder.DescTitle);

            var ordersArray = new string[4];
            ordersVariant.Keys.CopyTo(ordersArray, 0);

            string action = await Application.Current.MainPage
                .DisplayActionSheet("Сортировка"
                , Resource.Cancel, null, ordersArray);

            if (action == "Cancel")
                return;
            if (ordersVariant.TryGetValue(action, out SortOrder sort))
                _SortOrder = sort;
            await InitAsync();
        }

        public abstract Task InitAsync(CancellationToken ct = default);
        //public override void Unsubscribe();
        protected abstract Task OnCmdEdit(object item);
        protected abstract Task OnCmdDel();
        protected abstract Task OnCmdAdd();
    }

}
