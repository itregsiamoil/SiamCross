using System;
using System.Threading.Tasks;

namespace SiamCross.Services.StdDialog
{
    public sealed class StdDialogService
    {
        private static readonly Lazy<StdDialogService> _instance =
            new Lazy<StdDialogService>(() => new StdDialogService());
        public static StdDialogService Instance => _instance.Value;

        private readonly IFileOpenDialog _file_open_dlg;
        private StdDialogService()
        {
            _file_open_dlg = Xamarin.Forms.DependencyService.Get<IFileOpenDialog>();
        }
        public Task<string> ShowOpenDialog()
        {
            return _file_open_dlg.Show();
        }

    }
}
