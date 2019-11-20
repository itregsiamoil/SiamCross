using MvvmCross.Plugin.Messenger;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Events
{
    public class QuickReportReceived : MvxMessage
    {
        public QuickReportReceived(object sender) : base(sender)
        {

        }
    }
}
