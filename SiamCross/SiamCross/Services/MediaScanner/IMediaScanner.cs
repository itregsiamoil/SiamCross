using System.Threading.Tasks;

namespace SiamCross.Services.MediaScanner
{
    public interface IMediaScanner
    {
        Task<bool> Scan(string path);
    }
}
