using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiamCross.ViewModels
{
    public abstract class BaseVM : IViewModel
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;
        public void ChangeNotify([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}
