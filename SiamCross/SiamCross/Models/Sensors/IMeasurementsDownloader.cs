using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public interface IMeasurementsDownloader
    {
        Task Clear();
        Task<RespResult> Update();
        int Aviable();
        Task<IReadOnlyList<object>> Download(uint begin, uint end
            , Action<float> onStepProgress = null, Action<string> onStepInfo = null);
    }
}
