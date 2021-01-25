using System.Threading.Tasks;

namespace SiamCross.Services.StdDialog
{
    public interface IFileOpenDialog
    {
        Task<string> Show();
    }
}
