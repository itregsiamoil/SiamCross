using SiamCross.Models;
using SiamCross.Services;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
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

        public PositionInfoVM()
            : this(new PositionInfo())
        {
        }
        public PositionInfoVM(PositionInfo position)
        {
            _Model = position;
            EditCommand = new Command(DoEditCommand);
            Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
            AddField = new Command(DoAddNewFieldCommand);
        }

        public ObservableCollection<string> Fields { get; set; }
        public ICommand AddField { get; set; }


        public string Field
        {
            get => _Model.Field;
            set { _Model.Field = value; ChangeNotify(); }
        }
        public string Well
        {
            get => _Model.Well;
            set { _Model.Well = value; ChangeNotify(); }
        }
        public string Bush
        {
            get => _Model.Bush;
            set { _Model.Bush = value; ChangeNotify(); }
        }
        public string Shop
        {
            get => _Model.Shop;
            set { _Model.Shop = value; ChangeNotify(); }
        }
        public GeoLocationVM GeoLocation
        {
            get => new GeoLocationVM(_Model.Location);
            set { _Model.Location = value._Model; ChangeNotify(); }
        }

        public ICommand EditCommand { get; set; }
        private void DoEditCommand()
        {
            App.NavigationPage.Navigation
                .PushAsync(new PositionEditView(this));
        }

        protected void DoAddNewFieldCommand()
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(AddFieldPage))
                    {
                        App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                    }
                }
                else
                {
                    App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                }
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
