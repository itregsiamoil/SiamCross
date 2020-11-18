using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Tools
{
    public interface IAppVersionAndBuild
    {
        string GetVersionNumber();
        string GetBuildNumber();
    }
}
