using SiamCross.Models.Tools;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public interface ISettingsSaver
    {
        Task SaveSettings(MailSettingsData settings);

        Task<MailSettingsData> ReadSettings();
    }
}
