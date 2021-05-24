using System.Threading.Tasks;

namespace SiamCross.Services
{
    public static class Repo
    {
        static readonly FieldDir _FieldDir = new FieldDir();
        static readonly AttrDir _AttrDir = new AttrDir();

        public static FieldDir FieldDir => _FieldDir;
        public static AttrDir AttrDir => _AttrDir;


        public static async Task InitAsync()
        {
            await FieldDir.InitAsync();
            await AttrDir.InitAsync();
        }
    }
}
