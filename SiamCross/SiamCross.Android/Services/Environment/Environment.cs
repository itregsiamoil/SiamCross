using Android.Runtime;
using SiamCross.Droid.Utils.FileSystem;
using SiamCross.Services.Environment;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Environment))]
namespace SiamCross.Droid.Utils.FileSystem
{
    [Preserve(AllMembers = true)]
    public class Environment : IEnvironment
    {
        public string GetDir_Downloads()
        {
            string dw = Android.OS.Environment.DirectoryDownloads;
            return Android.OS.Environment.GetExternalStoragePublicDirectory(dw).AbsolutePath;
        }
        public string GetDir_Measurements()
        {
            return Path.Combine(GetDir_Downloads(), "SiamServiceMeasurement");
        }

    }
}