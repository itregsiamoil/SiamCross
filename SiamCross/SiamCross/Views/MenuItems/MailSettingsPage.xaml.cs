using SiamCross.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MailSettingsPage : BaseContentPage
    {
        private Task InitTask;
        private CancellationTokenSource Cts;
        private MailSettingsVM _vm;

        public MailSettingsPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            IsBusy = true;
            if (BindingContext is MailSettingsVM bindVM)
            {
                _vm = bindVM;
            }
            else
            {
                if (null == _vm)
                    _vm = new MailSettingsVM();
                BindingContext = _vm;
            }

            if (null != Cts)
            {
                if (Cts.IsCancellationRequested)
                {
                    Cts.Dispose();
                    Cts = new CancellationTokenSource();
                }
            }
            else
                Cts = new CancellationTokenSource();

            InitTask = Task.Run(() => VmInitAsync(Cts.Token));
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            IsBusy = true;
            base.OnDisappearing();
            InitTask = Task.Run(() => VmDeinitAsync(Cts.Token));
        }
        protected async Task VmInitAsync(CancellationToken ct)
        {
            await _vm.InitAsync(ct).ConfigureAwait(false);
            IsBusy = false;
        }
        protected async Task VmDeinitAsync(CancellationToken ct)
        {
            if (null == _vm)
                return;
            if (null != InitTask && !InitTask.IsCompleted)
            {
                Cts?.Cancel();
                await InitTask;
            }
            _vm.Unsubscribe();
            IsBusy = false;
        }
    }
}