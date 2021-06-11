using System;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BasePageVM : BaseVM, IDisposable
    {
        DisplayOrientation _Orientation = DeviceDisplay.MainDisplayInfo.Orientation;
        public bool IsLandscape => DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape;
        public bool IsPortrait => DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait;

        public bool _EnableOrintationNotify = false;
        public bool EnableOrintationNotify
        {
            get => _EnableOrintationNotify;
            set
            {
                if (_EnableOrintationNotify == value)
                    return;
                _EnableOrintationNotify = value;
                if (_EnableOrintationNotify)
                {
                    _Orientation = DeviceDisplay.MainDisplayInfo.Orientation;
                    DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
                    OrintationNotify();
                }
                else
                    DeviceDisplay.MainDisplayInfoChanged -= DeviceDisplay_MainDisplayInfoChanged;
            }
        }
        public void OrintationNotify()
        {
            ChangeNotify(nameof(IsLandscape));
            ChangeNotify(nameof(IsPortrait));
        }
        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            if (null != e && e.DisplayInfo.Orientation == _Orientation)
                return;
            _Orientation = e.DisplayInfo.Orientation;
            ChangeNotify(nameof(IsLandscape));
            ChangeNotify(nameof(IsPortrait));
        }
        public virtual void Unsubscribe()
        {
            EnableOrintationNotify = false;
        }
        public virtual void Dispose()
        {
            Unsubscribe();
        }
        public static void RaiseCanExecuteChanged(ICommand command)
        {
            if (command is AsyncCommand acmd)
                acmd.RaiseCanExecuteChanged();
            else if (command is Command cmd)
                cmd.ChangeCanExecute();
        }
    }
}
