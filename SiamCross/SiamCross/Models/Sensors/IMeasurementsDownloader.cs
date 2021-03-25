using SiamCross.Models.Connection.Protocol;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public interface IMeasurementsDownloader
    {
        Task Clear();
        Task<RespResult> Update();

    }
}
