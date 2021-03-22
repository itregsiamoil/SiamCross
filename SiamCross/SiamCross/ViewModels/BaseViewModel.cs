using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiamCross.ViewModels
{
    public abstract class BaseVM : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void ChangeNotify([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
