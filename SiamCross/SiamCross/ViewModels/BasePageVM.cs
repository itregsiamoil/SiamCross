using System;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BasePageVM : BaseVM, IDisposable
    {
        public bool IsLandscape => DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape;
        public bool IsPortrait => DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait;

        public abstract void Unsubscribe();
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
