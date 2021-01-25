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
    public class AddFieldViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public AddFieldViewModel()
        {
            Add = new Command(SaveField);
        }

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private void SaveField()
        {
            if (FieldName == null || FieldCode == null)
            {
                ToastService.Instance.LongAlert(Resource.FillInAllTheFields);
                return;
            }
            else if (FieldName == "" || FieldCode == "")
            {
                ToastService.Instance.LongAlert(Resource.FillInAllTheFields);
                return;
            }

            try
            {
                HandbookData.Instance.AddField(FieldName, int.Parse(FieldCode));
                MessagingCenter.Send<AddFieldViewModel>(this, "Refresh");
                App.NavigationPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveField command handler" + "\n");
                throw;
            }
        }
    }
}
