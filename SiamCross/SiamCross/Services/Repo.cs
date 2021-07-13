using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public static class Repo
    {
        static readonly FieldDir _FieldDir = new FieldDir();
        static readonly AttrDir _AttrDir = new AttrDir();
        static readonly SoundSpeedDir _SoundSpeedDir = new SoundSpeedDir();
        static readonly MailSettingsDir _MailSettingsDir = new MailSettingsDir();
        static readonly Config _Config = new Config();

        public static FieldDir FieldDir => _FieldDir;
        public static AttrDir AttrDir => _AttrDir;
        public static SoundSpeedDir SoundSpeedDir => _SoundSpeedDir;
        public static MailSettingsDir MailSettingsDir => _MailSettingsDir;
        public static Config Config => _Config;

        public static async Task InitAsync()
        {
            List<Task> t = new List<Task>(5)
            {
                FieldDir.InitAsync(),
                AttrDir.InitAsync(),
                SoundSpeedDir.InitAsync(),
                MailSettingsDir.InitAsync(),
                Config.InitAsync()
            };
            try
            {
                await Task.WhenAll(t);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
