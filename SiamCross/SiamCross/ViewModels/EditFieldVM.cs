using SiamCross.Services;
using SiamCross.Services.Toast;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class EditFieldVM : BaseVM
    {
        FieldItem _SavedField;
        public EditFieldVM(FieldItem field = null)
        {
            Add = new AsyncCommand(SaveFieldAsync
                , (Func<object, bool>)null, null, false, false);
            _SavedField = field;
            if (null != _SavedField)
            {
                FieldName = field.Title;
                FieldCode = field.Id.ToString();
            }
        }

        public string FieldName { get; set; }
        public string FieldCode { get; set; }

        public ICommand Add { get; set; }

        private async Task SaveFieldAsync()
        {

            try
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
                if (null != _SavedField)
                    await Repo.FieldDir.DeleteAsync(_SavedField.Id);
                await Repo.FieldDir.AddAsync(FieldName, uint.Parse(FieldCode));
                await App.NavigationPage.Navigation.PopAsync();
            }
            catch (Exception)
            {
                //throw;
            }
        }
    }
}
