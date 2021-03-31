using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
            AddField = new Command(OpenAddFieldsPage);
            Remove = new Command(RemoveField);
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
                Dictionary<string, long> fieldDict = HandbookData.Instance.GetFieldDictionary();
                foreach (KeyValuePair<string, long> field in fieldDict)
                {
                    Fields.Add(new FieldPair(field.Key, field.Value.ToString()));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
                throw;
            }
        }

        public ObservableCollection<FieldPair> Fields { get; set; }
        public object SelectedField { get; set; }

        public ICommand AddField { get; set; }
        public ICommand Remove { get; set; }

        private void RemoveField()
        {
            try
            {
                if (SelectedField != null)
                {
                    HandbookData.Instance.RemoveField((SelectedField as FieldPair).Key);
                    Update();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RemoveField command handler" + "\n");
                throw;
            }
        }
        private void OpenAddFieldsPage()
        {
            try
            {
                System.Collections.Generic.IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(AddFieldPage))
                    {
                        App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                    }
                }
                else
                {
                    App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
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
