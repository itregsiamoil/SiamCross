using System.Threading.Tasks;

namespace SiamCross.Services
{
    static public class Repo
    {
        static readonly FieldDir _FieldDir = new FieldDir();

        static public FieldDir FieldDir => _FieldDir;


        static public async Task Init()
        {
            await FieldDir.Init();
        }
    }
}
