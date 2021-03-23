using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public interface IMeasurementsDownloader
    {
        Task Clear();
        Task Update();
        int Aviable();
        Task<object> Download(int begin, int end
            , Action<float> onStepProgress = null, Action<string> onStepInfo = null);
    }
}
