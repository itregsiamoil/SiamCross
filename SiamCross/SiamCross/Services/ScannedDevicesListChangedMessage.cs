using MvvmCross.Plugin.Messenger;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services
{
    class ScannedDevicesListChangedMessage : MvxMessage
    {
        public ScannedDevicesListChangedMessage(object sender) : base(sender)
        {

        }
    }
}
