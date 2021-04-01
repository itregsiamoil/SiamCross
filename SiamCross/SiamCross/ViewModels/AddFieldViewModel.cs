using SiamCross.Services;
using SiamCross.Services.Toast;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class AddFieldViewModel : BaseVM
    {
        public AddFieldViewModel()
        {
            Add = new AsyncCommand(SaveFieldAsync
                , (Func<object, bool>)null, null, false, false);
        }

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private async Task SaveFieldAsync()
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

                await Repo.FieldDir.AddAsync(FieldName, uint.Parse(FieldCode));
                MessagingCenter.Send<AddFieldViewModel>(this, "Refresh");
                await App.NavigationPage.Navigation.PopModalAsync();
            }
            catch (Exception)
            {
                //throw;
            }
        }
    }
}
