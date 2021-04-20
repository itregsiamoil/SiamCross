using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Services.RepositoryTables;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class FieldsCollection : ObservableCollection<string>
    {
        public FieldsCollection()
        { }
    }

    public class PositionVM : BaseVM, IDisposable
    {
        readonly SensorPosition _Model;

        public ISensor Sensor { get; }
        public ICommand CmdEdit { get; }
        public ICommand CmdMakeNew { get; }
        public ICommand CmdLoad { get; }
        public ICommand CmdSave { get; }

        readonly ObservableCollection<FieldItem> _Fields = new ObservableCollection<FieldItem>();
        public ObservableCollection<FieldItem> Fields => _Fields;
        private void FieldList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _Fields.Clear();
            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o));
            ChangeNotify(nameof(SelectedField));
        }
        private void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != _Model)
                return;
            if ("Saved" == e.PropertyName)
            {
                ChangeNotify(nameof(FieldName));
                ChangeNotify(nameof(FieldId));
                ChangeNotify(nameof(Well));
                ChangeNotify(nameof(Bush));
                ChangeNotify(nameof(Shop));
            }
            if ("Current" == e.PropertyName)
            {
                ChangeNotify(nameof(SelectedField));
                ChangeNotify(nameof(CurrentFieldId));
                ChangeNotify(nameof(CurrentWell));
                ChangeNotify(nameof(CurrentBush));
                ChangeNotify(nameof(CurrentShop));
            }

        }

        public PositionVM(ISensor sensor)
        {
            Sensor = sensor;
            _Model = Sensor.Model.Position;
            _Model.PropertyChanged += StorageModel_PropertyChanged;

            CmdEdit = CmdEdit = new AsyncCommand(ShowEditor
                , (Func<object, bool>)null, null, false, false);
            CmdMakeNew = _Model.CmdMakeNew;
            CmdLoad = _Model.CmdLoad;
            CmdSave = _Model.CmdSave;

            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o));
            Repo.FieldDir.FieldList.CollectionChanged += FieldList_CollectionChanged;
        }
        async Task ShowEditor()
        {
            var cmd = PageNavigator.CreateAsyncCommand(() => new PositionVM(Sensor));
            await cmd.ExecuteAsync();
        }
        public void Dispose()
        {
            _Model.PropertyChanged -= StorageModel_PropertyChanged;
        }

        public string FieldId => _Model.Saved.Field.ToString();
        public string FieldName
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Model.Saved.Field, out FieldItem item))
                    return item.Title;
                return string.Empty;
            }
        }
        public string Well => _Model.Saved.Well;
        public string Bush => _Model.Saved.Bush;
        public string Shop => _Model.Saved.Shop.ToString();

        public FieldItem SelectedField
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Model.Current.Field, out FieldItem item))
                    return item;
                return null;
            }
            set
            {
                if (null == value)
                    return;
                if (!Repo.FieldDir.DictByTitle.TryGetValue(value.Title, out FieldItem item))
                    return;
                if (_Model.Current.Field == item.Id)
                    return;
                _Model.Current.Field = item.Id;
                ChangeNotify();
                ChangeNotify(nameof(CurrentFieldId));
            }
        }
        public string CurrentFieldId
        {
            get => _Model.Current.Field.ToString();
            set
            {
                if (string.IsNullOrEmpty(value) || !uint.TryParse(value, out uint id))
                    return;
                _Model.Current.Field = id;
                ChangeNotify();
                ChangeNotify(nameof(SelectedField));
            }
        }
        public string CurrentWell
        {
            get => _Model.Current.Well;
            set
            {
                _Model.Current.Well = value;
                ChangeNotify();
            }
        }
        public string CurrentBush
        {
            get => _Model.Current.Bush;
            set
            {
                _Model.Current.Bush = value;
                ChangeNotify();
            }
        }
        public string CurrentShop
        {
            get => _Model.Current.Shop.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    _Model.Current.Shop = val;
                    ChangeNotify();
                }
            }
        }

    }
}
