using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SoundSpeedViewModel : BaseViewModel, IViewModel
    {
        public ObservableCollection<SoundSpeedModel> SoundSpeedList { get; set; }
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public SoundSpeedViewModel()
        {
            SoundSpeedList = new ObservableCollection<SoundSpeedModel>();
            Update();
                
            AddSoundSpeed = new Command(AddSound);

            MessagingCenter.Subscribe<SoundSpeedViewViewModel>(
                this,
                "Refresh",
                (sender) =>  { Update(); });
        }

        private void Update()
        {
            try
            {
                SoundSpeedList.Clear();
                LoadSoundSpeeds();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method");
                throw;
            }
        }

        public ICommand AddSoundSpeed { get; set; }

        private void AddSound()
        {
            //todo
        }

        private void LoadSoundSpeeds()
        {
            var soundSpeedList = HandbookData.Instance.GetSoundSpeedList();
            foreach(var soundItem in soundSpeedList)
            {
                SoundSpeedList.Add(soundItem);
            }
        }
    }
}
