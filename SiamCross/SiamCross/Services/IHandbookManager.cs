using SiamCross.Models.Tools;
using System.Collections.Generic;

namespace SiamCross.Services
{
    public interface IHandbookManager
    {
        List<SoundSpeedModel> LoadSoundSpeeds();
        void SaveSoundSpeeds(List<SoundSpeedModel> soundSpeedsList);
    }
}
