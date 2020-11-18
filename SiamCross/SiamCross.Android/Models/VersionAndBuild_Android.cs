using Android.Content.PM;  
using VersionAndBuildNumber.Droid.DependencyServices;  
using Xamarin.Forms;  

[assembly: Dependency(typeof(VersionAndBuild_Android))]  
namespace VersionAndBuildNumber.Droid.DependencyServices
{
    public class VersionAndBuild_Android : SiamCross.Models.Tools.IAppVersionAndBuild
    {
        PackageInfo _appInfo;
        public VersionAndBuild_Android()
        {
            var context = Android.App.Application.Context;
            _appInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
        }
        public string GetVersionNumber()
        {
            return _appInfo.VersionName;
        }
        public string GetBuildNumber()
        {
            return _appInfo.VersionCode.ToString();
        }
    }
}