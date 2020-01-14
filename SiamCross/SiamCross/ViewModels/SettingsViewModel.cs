using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.ViewModels
{
    public class SettingsViewModel : BaseViewModel, IViewModel
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string SmtpAddress { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SettingsViewModel()
        {

        }
    }
}
