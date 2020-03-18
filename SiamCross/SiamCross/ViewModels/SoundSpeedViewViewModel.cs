using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SoundSpeedViewViewModel : IViewModel
    {
        public SoundSpeedViewViewModel(SoundSpeedModel soundSpeed)
        {
            _targetSoundSpeed = soundSpeed;
            Edit = new Command(TrySaveEdits);
            _toater = DependencyService.Get<IToast>();
            Name = soundSpeed.Name;
            Code = soundSpeed.Code.ToString();
        }

        private IToast _toater;

        private SoundSpeedModel _targetSoundSpeed;

        public string Name { get; set; }

        public string Code { get; set; }

        public ICommand Edit { get; set; }

        private void TrySaveEdits()
        {
            if(string.IsNullOrWhiteSpace(Name) || 
               string.IsNullOrWhiteSpace(Code))
            {
                _toater.Show(Resource.FillInAllTheFields);
                return;
            }

            HandbookData.Instance.RemoveSoundSpeed(_targetSoundSpeed);
            _targetSoundSpeed.Code = int.Parse(Code);
            _targetSoundSpeed.Name = Name;
            HandbookData.Instance.AddSoundSpeed(_targetSoundSpeed);
            MessagingCenter.Send<SoundSpeedViewViewModel>(this, "Refresh");
            App.NavigationPage.Navigation.PopAsync();

            _targetSoundSpeed.Code = int.Parse(Code);
            _targetSoundSpeed.Name = Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
