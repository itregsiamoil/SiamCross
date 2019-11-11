using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SiamCross.ViewModels
{
    public class ControlPanelPageViewModel : BaseViewModel
    {
        public ObservableCollection<string> ListViewDevices { get; set; }
    }
}
