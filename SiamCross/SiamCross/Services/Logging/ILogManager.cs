using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services.Logging
{
    public interface ILogManager
    {
        NLog.Logger GetLog([System.Runtime.CompilerServices.CallerFilePath]string callerFilePath = "");
    }
}
