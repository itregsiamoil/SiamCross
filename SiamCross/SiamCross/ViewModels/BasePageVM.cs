using System;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BasePageVM : BaseVM, IDisposable
    {
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
