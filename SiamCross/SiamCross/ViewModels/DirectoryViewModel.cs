using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class DirectoryViewModel : BaseVM
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public DirectoryViewModel()
        {
            Fields = new ObservableCollection<FieldPair>();
            Update();
            AddCommand = new AsyncCommand(OpenAddFieldsPageAsync
                , (Func<object, bool>)null, null, false, false);
            RemoveCommand = new AsyncCommand(RemoveFieldAsync
                , (Func<object, bool>)null, null, false, false);


            MessagingCenter.Subscribe<AddFieldViewModel>(
                this,
                "Refresh",
                (sender) =>
                {
                    Update();
                });
        }

        private void Update()
        {
            try
            {
                Fields.Clear();
                Repo.FieldDir.FieldList.ForEach(o =>
                    Fields.Add(new FieldPair(o.Title, o.Id.ToString())));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
                throw;
            }
        }

        public ObservableCollection<FieldPair> Fields { get; set; }
        public object SelectedField { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        private async Task RemoveFieldAsync()
        {
            try
            {
                if (!(SelectedField is FieldPair item))
                    return;
                await Repo.FieldDir.DeleteAsync(item.Key);
                Update();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RemoveField command handler" + "\n");
                throw;
            }
        }
        private async Task OpenAddFieldsPageAsync()
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(AddFieldPage))
                    {
                        await App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                    }
                }
                else
                {
                    await App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OpenAddFieldsPage command handler" + "\n");
                throw;
            }
        }
    }

    public class FieldPair
    {
        public FieldPair(string key, string code)
        {
            Key = key;
            Code = code;
        }

        public string Key { get; set; }
        public string Code { get; set; }
    }
}
