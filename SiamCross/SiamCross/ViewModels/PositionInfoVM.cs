using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

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


    public class PositionInfoVM : BaseVM
    {
        protected PositionInfo _Model = new PositionInfo();
        public PositionInfo Model => _Model;

        public ICommand EditCommand { get; set; }
        public ICommand AddFieldCommand { get; set; }

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

            Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
            MessagingCenter.Subscribe<AddFieldViewModel>(this,
                    "Refresh", (sender) => UpdateFields());

        }

        public ObservableCollection<string> Fields { get; private set; }

        private void UpdateFields()
        {
            try
            {
                Fields.Clear();
                IEnumerable<string> fieldList = HandbookData.Instance.GetFieldList();
                foreach (string field in fieldList)
                    Fields.Add(field);
            }
            catch (Exception) { }
        }


        public string Field
        {
            get => _Model.Field.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    _Model.Field = val;
                    ChangeNotify();
                }
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
