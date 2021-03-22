﻿using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Services.Toast;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class AddFieldViewModel : BaseVM
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public AddFieldViewModel()
        {
            Add = new Command(SaveFieldAsync);
        }

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private async void SaveFieldAsync()
        {
            if (FieldName == null || FieldCode == null || FieldName == "" || FieldCode == "")
            {
                ToastService.Instance.LongAlert(Resource.FillInAllTheFields);
                return;
            }

            if (FieldCode.Length > 4)
            {
                bool accept = await Application.Current.MainPage.DisplayAlert(Resource.Attention
                    , Resource.WarningFieldIdOverflow, Resource.Ok, Resource.Cancel);
                if (!accept)
                    return;
            }

            try
            {
                HandbookData.Instance.AddField(FieldName, int.Parse(FieldCode));
                MessagingCenter.Send<AddFieldViewModel>(this, "Refresh");
                await App.NavigationPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveField command handler" + "\n");
                throw;
            }
        }
    }
}
