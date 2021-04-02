using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Services.RepositoryTables;
using SiamCross.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public class GeoLocationVM : BaseVM
    {
        public GeoLocation _Model;
        public GeoLocation Model => _Model;
        public GeoLocationVM()
            : this(new GeoLocation())
        {
        }
        public GeoLocationVM(GeoLocation geoLocation)
        {
            _Model = geoLocation;
        }
        public string Altitude
        {
            get => _Model.Altitude.ToString();
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _Model.Altitude = val;
                    ChangeNotify();
                }
            }
        }
        public string Latitude
        {
            get => _Model.Latitude.ToString();
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _Model.Latitude = val;
                    ChangeNotify();
                }
            }
        }
        public string Longitude
        {
            get => _Model.Longitude.ToString();
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _Model.Longitude = val;
                    ChangeNotify();
                }
            }
        }
        public string Accuracy
        {
            get => _Model.Accuracy.ToString();
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _Model.Accuracy = val;
                    ChangeNotify();
                }
            }
        }
    }


    public class FieldsCollection : ObservableCollection<string>
    {
        public FieldsCollection()
        { }
    }




    public class PositionInfoVM : BaseVM
    {
        protected PositionInfo _Model = new PositionInfo();
        public PositionInfo Model => _Model;

        public ICommand EditCommand { get; set; }
        public ICommand AddFieldCommand { get; set; }

        ObservableCollection<FieldItem> _Fields = new ObservableCollection<FieldItem>();
        public ObservableCollection<FieldItem> Fields => _Fields;
        private void FieldList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _Fields.Clear();
            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o));
            ChangeNotify(nameof(SelectedField));
        }
        public PositionInfoVM()
            : this(new PositionInfo())
        {
        }
        public PositionInfoVM(PositionInfo position)
        {
            _Model = position;
            EditCommand = BaseSensor.CreateAsyncCommand(() => this);
            AddFieldCommand = new AsyncCommand(DoAddNewFieldCommand
                , (Func<object, bool>)null, null, false, false);

            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o));
            Repo.FieldDir.FieldList.CollectionChanged += FieldList_CollectionChanged;
        }
        public FieldItem SelectedField
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Model.Field, out FieldItem item))
                    return item;
                return null;
            }
            set
            {
                if (null == value)
                    return;
                if (!Repo.FieldDir.DictByTitle.TryGetValue(value.Title, out FieldItem item))
                    return;
                if (_Model.Field == item.Id)
                    return;
                _Model.Field = item.Id;
                ChangeNotify();
                ChangeNotify(nameof(FieldId));
                ChangeNotify(nameof(FieldName));
            }
        }
        public string FieldId
        {
            get => _Model.Field.ToString();
            set
            {
                if (string.IsNullOrEmpty(value) || !uint.TryParse(value, out uint id))
                    return;
                _Model.Field = id;
                ChangeNotify();
                ChangeNotify(nameof(FieldName));
                ChangeNotify(nameof(SelectedField));
            }
        }
        public string FieldName
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Model.Field, out FieldItem item))
                    return item.Title;
                return string.Empty;
            }
        }

        public string Well
        {
            get => _Model.Well.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    _Model.Well = val;
                    ChangeNotify();
                }
            }
        }
        public string Bush
        {
            get => _Model.Bush.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    _Model.Bush = val;
                    ChangeNotify();
                }
            }
        }
        public string Shop
        {
            get => _Model.Shop.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    _Model.Shop = val;
                    ChangeNotify();
                }
            }
        }
        public GeoLocationVM GeoLocation
        {
            get => new GeoLocationVM(_Model.Location);
            set { _Model.Location = value._Model; ChangeNotify(); }
        }
        protected async Task DoAddNewFieldCommand()
        {
            try
            {
                await App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                //throw;
            }
        }

    }
}
