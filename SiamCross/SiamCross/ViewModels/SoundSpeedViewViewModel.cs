﻿using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Toast;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SoundSpeedViewViewModel : BaseVM
    {
        public SoundSpeedViewViewModel(SoundSpeedModel soundSpeed)
        {
            _targetSoundSpeed = soundSpeed;
            Edit = new Command(TrySaveEdits);
            Name = soundSpeed.Name;
            Code = soundSpeed.Code.ToString();

            Points = _targetSoundSpeed.LevelSpeedTable;

            MinGraphX = Math.Round(GetMinimumX(), 1).ToString();
            MaxGraphX = Math.Round(GetMaximumX(), 1).ToString();
            MinGraphY = Math.Round(GetMinimumY(), 1).ToString();
            MaxGraphY = Math.Round(GetMaximumY(), 1).ToString();
        }

        private readonly SoundSpeedModel _targetSoundSpeed;

        public List<KeyValuePair<float, float>> Points { get; private set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public ICommand Edit { get; set; }

        public string MinGraphX { get; set; }

        public string MinGraphY { get; set; }

        public string MaxGraphX { get; set; }

        public string MaxGraphY { get; set; }

        public void Delete()
        {
            try
            {
                HandbookData.Instance.RemoveSoundSpeed(_targetSoundSpeed);
                MessagingCenter.Send<SoundSpeedViewViewModel>(this, "Refresh");
            }
            catch { }
            App.NavigationPage.Navigation.PopAsync();
        }

        private void TrySaveEdits()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
               string.IsNullOrWhiteSpace(Code))
            {
                ToastService.Instance.LongAlert(Resource.FillInAllTheFields);
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

        public double GetMaximumX()
        {
            double max = Points[0].Key;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Key > max)
                {
                    max = pair.Key;
                }
            }

            return max;
        }

        public double GetMinimumX()
        {
            double min = Points[0].Key;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Key < min)
                {
                    min = pair.Key;
                }
            }

            return min;
        }

        public double GetMinimumY()
        {
            double min = Points[0].Value;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Value < min)
                {
                    min = pair.Value;
                }
            }

            return min;
        }

        public double GetMaximumY()
        {
            double max = Points[0].Value;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Value > max)
                {
                    max = pair.Value;
                }
            }

            return max;
        }

    }
}
