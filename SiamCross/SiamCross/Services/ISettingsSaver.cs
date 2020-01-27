using SiamCross.Models.Tools;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public interface ISettingsSaver
    {
        Task SaveSettings(SettingsParameters settings);

        Task<SettingsParameters> ReadSettings();
    }
}
