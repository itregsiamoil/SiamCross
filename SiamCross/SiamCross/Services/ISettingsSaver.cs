using SiamCross.Models.Tools;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public interface ISettingsSaver
    {
        Task SaveSettings(SettingsParameters settings);

        Task<SettingsParameters> ReadSettings();
    }
}
