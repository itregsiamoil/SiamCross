using System;
using System.Threading.Tasks;

namespace SiamCross.Services.MediaScanner
{
    public sealed class MediaScannerService : IMediaScanner
    {
        private static readonly Lazy<MediaScannerService> _instance =
            new Lazy<MediaScannerService>(() => new MediaScannerService());
        public static IMediaScanner Instance => _instance.Value;

        private readonly IMediaScanner _object;
        private MediaScannerService()
        {
            _object = Xamarin.Forms.DependencyService.Get<IMediaScanner>();
        }
        public Task<bool> Scan(string path)
        {
            return _object.Scan(path);
        }
    }
}
