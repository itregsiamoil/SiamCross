using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class AddFieldViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public AddFieldViewModel()
        {
            Add = new Command(SaveField);
            _toater = DependencyService.Get<IToast>();
        }

        private IToast _toater;

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private void SaveField()
        {
            if (FieldName == null || FieldCode == null)
            {
                _toater.Show(Resource.FillInAllTheFields);
                return;
            }
            else if (FieldName == "" || FieldCode == "")
            {
                _toater.Show(Resource.FillInAllTheFields);
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
                _logger.Error(ex, "SaveField command handler");
                throw;
            }
        }
    }
}
