using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class AddFieldViewModel : BaseViewModel, IViewModel
    {
        public AddFieldViewModel()
        {
            Add = new Command(SaveField);
        }

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private void SaveField()
        {
            HandbookData.Instance.AddField(FieldName, int.Parse(FieldCode));
            MessagingCenter.Send<AddFieldViewModel>(this, "Refresh");
            App.NavigationPage.Navigation.PopModalAsync();
        }
    }
}
