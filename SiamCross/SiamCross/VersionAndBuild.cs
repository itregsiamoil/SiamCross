using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(SiamCross.VersionAndBuild))]
namespace SiamCross
{
    public class VersionAndBuild : SiamCross.Models.Tools.IAppVersionAndBuild
    {
        //PackageInfo _appInfo;
        public VersionAndBuild()
        {
            //var context = Android.App.Application.Context;
            //_appInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
        }
        public string GetVersionNumber()
        {
            return AppInfo.VersionString;
        }
        public string GetBuildNumber()
        {
            return AppInfo.BuildString;
        }
    }
}