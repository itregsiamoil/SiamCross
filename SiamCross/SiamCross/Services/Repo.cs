using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public static class Repo
    {
        static readonly FieldDir _FieldDir = new FieldDir();
        static readonly AttrDir _AttrDir = new AttrDir();
        static readonly SoundSpeedDir _SoundSpeedDir = new SoundSpeedDir();

        public static FieldDir FieldDir => _FieldDir;
        public static AttrDir AttrDir => _AttrDir;
        public static SoundSpeedDir SoundSpeedDir => _SoundSpeedDir;


        public static async Task InitAsync()
        {
            List<Task> t = new List<Task>(3)
            {
                FieldDir.InitAsync(),
                AttrDir.InitAsync(),
                SoundSpeedDir.InitAsync()
            };
            await Task.WhenAll(t);
        }
    }
}
