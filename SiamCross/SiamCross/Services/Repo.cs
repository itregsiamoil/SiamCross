using System.Threading.Tasks;

namespace SiamCross.Services
{
    public static class Repo
    {
        static readonly FieldDir _FieldDir = new FieldDir();

        public static FieldDir FieldDir => _FieldDir;


        public static async Task Init()
        {
            await FieldDir.Init();
        }
    }
}
