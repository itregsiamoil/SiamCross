using Android.Runtime;
using SiamCross.Services.StdDialog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;


namespace SiamCross.Droid.Services.StdDialog
{
    [Preserve(AllMembers = true)]
    internal class FileOpenDialog : IFileOpenDialog
    {
        public async Task<string> Show()
        {
            PickOptions options = new PickOptions
            {
                FileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android,null },
                }),
                PickerTitle = "Select a file to open"
            };

            FileResult result = await FilePicker.PickAsync(options);
            return result?.FullPath;
        }
    }
}