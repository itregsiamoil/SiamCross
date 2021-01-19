using SiamCross.Models.Tools;
using System.Collections.Generic;

namespace SiamCross.Services
{
    public interface IHandbookManager
    {
        Dictionary<string, int> LoadFields();
        List<SoundSpeedModel> LoadSoundSpeeds();
        void SaveFields(Dictionary<string, int> fieldDict);
        void SaveSoundSpeeds(List<SoundSpeedModel> soundSpeedsList);
    }
}
