using Android.Media;
using Android.Runtime;
using SiamCross.Services.MediaScanner;
using System.Threading;
using System.Threading.Tasks;

//[assembly: Xamarin.Forms.Dependency(typeof(IMediaScannerNotifyer))]
namespace SiamCross.Droid.Services.MediaScanner
{
    internal class MyMediaScannerConnectionClient
        : Java.Lang.Object
        , MediaScannerConnection.IMediaScannerConnectionClient
    {
        private TaskCompletionSource<bool> _tcs;
        private CancellationTokenSource _cts;
        private string mFilename;
        private string mMimetype;
        public readonly MediaScannerConnection mMediaConn;
        public MyMediaScannerConnectionClient()
        {
            Android.Content.Context context = Android.App.Application.Context;
            mMediaConn = new MediaScannerConnection(context, this);
        }
        public void OnMediaScannerConnected()
        {
            System.Diagnostics.Debug.WriteLine($"MediaScanner connected - {mFilename}");
            mMediaConn.ScanFile(mFilename, mMimetype);
        }
        public void OnScanCompleted(string path, Android.Net.Uri uri)
        {
            System.Diagnostics.Debug.WriteLine($"MediaScanner scan completed - {path}");
            mMediaConn.Disconnect();
            _tcs?.TrySetResult(true);
        }

        public void StartScan(string filename
            , string mimetype, TaskCompletionSource<bool> tcs)
        {
            mFilename = filename;
            mMimetype = mimetype;
            _tcs = tcs;
            _cts = new CancellationTokenSource(50000);
            _cts.Token.Register(() =>
            {
                System.Diagnostics.Debug.WriteLine($"MediaScanner scan dropped - {filename}");
                _tcs?.TrySetResult(false);
            });
            mMediaConn.Connect();
        }

        public void Stop()
        {
            _cts?.Dispose();
            _tcs = null;
            _cts = null;
            //mMediaConn.Dispose();
            System.Diagnostics.Debug.WriteLine("MediaScanner Stop");
        }
    } // internal class MyMediaScannerConnectionClient

    [Preserve(AllMembers = true)]
    public class MediaScanner : IMediaScanner
    {
        private readonly MyMediaScannerConnectionClient _scanner
            = new MyMediaScannerConnectionClient();
        public MediaScanner()
        {
        }
        public async Task<bool> Scan(string path)
        {
            TaskCompletionSource<bool> tcs
                = new TaskCompletionSource<bool>();
            _scanner.StartScan(path, null, tcs);
            bool ret = await tcs.Task;
            _scanner.Stop();
            return ret;
        }
    }//public class MediaScannerNotifyer
    /*
     * в таком варианте не работает колбэк для одног файла
     * https://www.grokkingandroid.com/adding-files-to-androids-media-library-using-the-mediascanner/
    [Preserve(AllMembers = true)]
    internal class ScanCompletedListener : Java.Lang.Object, MediaScannerConnection.IOnScanCompletedListener
    {
        private readonly TaskCompletionSource<bool> _TaskCompletionSource;
        public ScanCompletedListener(TaskCompletionSource<bool> tsc)
            : base()
        {
            _TaskCompletionSource = tsc;
            CancellationTokenSource cts = new CancellationTokenSource(10000);
            cts.Token.Register(() =>
            {
                System.Diagnostics.Debug.WriteLine(@"scan dropped {path}");
                _TaskCompletionSource?.TrySetResult(false);
                //tsc?.TrySetException(new OperationCanceledException());
            });
        }
        public void OnScanCompleted(string path, global::Android.Net.Uri uri)
        {
            System.Diagnostics.Debug.WriteLine($"scan completed. path is {path}");
            _TaskCompletionSource?.TrySetResult(true);
        }
    }
    public async Task<bool> NotifyMediaStorageAsync(string path)
    {
        TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
        ScanCompletedListener lisener = new ScanCompletedListener(tsc);



        MediaScannerConnection.ScanFile(Android.App.Application.Context,
            new String[] { path }, null, lisener);

        bool result = await tsc.Task;

        return result;
    }
    */
}
