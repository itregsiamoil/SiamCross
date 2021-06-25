using SiamCross.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    public partial class SoundSpeedListPage
    {
        private Task InitTask;
        private CancellationTokenSource Cts;
        SoundSpeedListVM _vm;
        public SoundSpeedListPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            if (null == _vm)
                _vm = new SoundSpeedListVM();
            if (!(BindingContext is SoundSpeedListVM))
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
                Task.Run(async () =>
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
        protected override bool OnBackButtonPressed()
        {
            return _vm.OnBackButton();
        }
    }
}