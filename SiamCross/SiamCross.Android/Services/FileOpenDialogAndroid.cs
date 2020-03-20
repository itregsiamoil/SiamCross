using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SiamCross.Services.UserOutput;
using Com.Obsez.Android.Lib.Filechooser;
using static Com.Obsez.Android.Lib.Filechooser.Listeners;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace SiamCross.Droid.Services
{
    public class FileOpenDialogAndroid : IFileOpenDialog
    {
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