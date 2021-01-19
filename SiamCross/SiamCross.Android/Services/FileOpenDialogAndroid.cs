using Android.Runtime;
using Com.Obsez.Android.Lib.Filechooser;
using SiamCross.Services.UserOutput;
using System.Threading.Tasks;
using static Com.Obsez.Android.Lib.Filechooser.Listeners;

namespace SiamCross.Droid.Services
{
    public class FileOpenDialogAndroid : IFileOpenDialog
    {
        [Preserve(AllMembers = true)]
        public Task<string> Show()
        {
            TaskCompletionSource<string> filePikcerCompletion = new TaskCompletionSource<string>();

            ChooserDialog chooserDialog = new ChooserDialog(MainActivity.CurrentActivity)
                .WithStringResources(SiamCross.Resource.ChooseAFile,
                    SiamCross.Resource.Choose, SiamCross.Resource.Cancel)
                .WithOptionStringResources(SiamCross.Resource.NewFolder,
                    SiamCross.Resource.Delete, SiamCross.Resource.Cancel, SiamCross.Resource.Ok)
                .EnableOptions(true)
                .DisplayPath(true)
                .WithChosenListener((dir, dirFile) =>
                {
                    filePikcerCompletion.SetResult(dir);
                })
                .Show();

            return filePikcerCompletion.Task;
        }
    }
}