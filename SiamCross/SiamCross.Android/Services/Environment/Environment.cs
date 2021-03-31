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
        [System.Obsolete]
        public string GetDir_Downloads()
        {
            string dw = Android.OS.Environment.DirectoryDownloads;
            return Android.OS.Environment.GetExternalStoragePublicDirectory(dw).AbsolutePath;
        }

        [System.Obsolete]
        public string GetDir_Measurements()
        {
            string p = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            return Path.Combine(p, "Measurements");
            //return Path.Combine(GetDir_Downloads(), "SiamServiceMeasurement");
        }

        public string GetDir_LocalApplicationData()
        {
            string p = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            return p;
        }
    }
}