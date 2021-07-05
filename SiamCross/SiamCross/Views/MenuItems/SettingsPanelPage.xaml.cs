using SiamCross.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : BaseContentPage
    {
        private Task InitTask;
        private CancellationTokenSource Cts;
        private SettingsViewModel _vm;

        public SettingsPanelPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            if (null == _vm)
                _vm = new SettingsViewModel();
            if (!(BindingContext is SettingsViewModel))
                BindingContext = _vm;

            if (null == Cts || Cts.IsCancellationRequested)
                Cts = new CancellationTokenSource();
            InitTask = Task.Run(async () => await _vm.InitAsync(Cts.Token).ConfigureAwait(false));
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (null != _vm)
            {
                _ = Task.Run(async () =>
                {
                    if (null != InitTask && !InitTask.IsCompleted)
                    {
                        Cts?.Cancel();
                        await InitTask;
                    }
                    _vm.Unsubscribe();
                });
            }
        }
    }
}