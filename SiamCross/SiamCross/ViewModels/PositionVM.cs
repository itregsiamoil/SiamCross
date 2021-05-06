using SiamCross.Models;
using SiamCross.Services;
using SiamCross.Services.RepositoryTables;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{

    public class PositionVM : BasePageVM
    {
        readonly PositionModel _Model;
        readonly ObservableCollection<FieldItem> _Fields = new ObservableCollection<FieldItem>();
        void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != _Model)
                return;
            ChangeNotify(e.PropertyName);
        }
        void FieldList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _Fields.Clear();
            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o));
            ChangeNotify(nameof(Fields));
        }
        public PositionVM(PositionModel pos)
        {
            _Model = pos;
            _Model.PropertyChanged += StorageModel_PropertyChanged;
            Repo.FieldDir.FieldList.CollectionChanged += FieldList_CollectionChanged;
        }
        public override void Unsubscribe()
        {
            _Model.PropertyChanged -= StorageModel_PropertyChanged;
            Repo.FieldDir.FieldList.CollectionChanged -= FieldList_CollectionChanged;
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public ObservableCollection<FieldItem> Fields => _Fields;
        public string FieldId => _Model.FieldId.ToString();
        public string FieldName => _Model.FieldName;
        public string Well => _Model.Well;
        public string Bush => _Model.Bush;
        public string Shop => _Model.Shop.ToString();
        public FieldItem SelectedField
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Model.FieldId, out FieldItem item))
                    return item;
                return null;
            }
            set
            {
                if (null == value)
                    return;
                if (!Repo.FieldDir.DictByTitle.TryGetValue(value.Title, out FieldItem item))
                    return;
                if (_Model.FieldId == item.Id)
                    return;
                _Model.FieldId = item.Id;
                ChangeNotify();
                ChangeNotify(nameof(FieldId));
            }
        }
    }

}
