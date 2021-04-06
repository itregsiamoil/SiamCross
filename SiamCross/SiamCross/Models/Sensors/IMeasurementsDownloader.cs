using SiamCross.Models.Connection.Protocol;
using SiamCross.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface IMeasurementsDownloader
    {
        Task Clear();
        Task<RespResult> Update(CancellationToken token = default, IProgress<string> progress = null);

    }

    public class BaseMeasurementsDownloaderVM : BaseVM
    {
        public ICommand LoadFromDeviceCommand { get; set; }
    }
}
