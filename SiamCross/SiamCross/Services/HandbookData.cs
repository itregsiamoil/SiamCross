using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;

namespace SiamCross.Services
{
    public class HandbookData : IDisposable
    {
        private static readonly Lazy<HandbookData> _instance =
            new Lazy<HandbookData>(() => new HandbookData());

        public static HandbookData Instance => _instance.Value;
        private readonly IHandbookManager _handbookManager;

        private List<SoundSpeedModel> _soundSpeedList;

        private HandbookData()
        {
            _handbookManager = AppContainer.Container.Resolve<IHandbookManager>();
        }
        public void Dispose()
        {
            if (_soundSpeedList != null)
            {
                _handbookManager.SaveSoundSpeeds(_soundSpeedList);
            }
        }
        public List<SoundSpeedModel> GetSoundSpeedList()
        {
            if (_soundSpeedList == null)
            {
                _soundSpeedList = _handbookManager.LoadSoundSpeeds();
            }

            return _soundSpeedList;
        }
        public void AddSoundSpeed(SoundSpeedModel soundSpeed)
        {
            if (_soundSpeedList != null)
            {
                _soundSpeedList.Add(soundSpeed);
                _handbookManager.SaveSoundSpeeds(_soundSpeedList);
            }
        }
        public void RemoveSoundSpeed(SoundSpeedModel soundSpeed)
        {
            if (_soundSpeedList != null)
            {
                _soundSpeedList.Remove(soundSpeed);
            }

            _handbookManager.SaveSoundSpeeds(_soundSpeedList);
        }
    }
}
