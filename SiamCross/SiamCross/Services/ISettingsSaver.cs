using SiamCross.Models.Tools;

namespace SiamCross.Services
{
    //Связан с синглтоном Settings
    public interface ISettingsSaver
    {
        void SaveSettings(SettingsParameters settings);

        bool DoesSettingsFileExists();

        SettingsParameters ReadSettings();
    }
}
