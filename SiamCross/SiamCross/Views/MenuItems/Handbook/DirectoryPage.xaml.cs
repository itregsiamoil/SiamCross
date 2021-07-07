using SiamCross.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    public partial class DirectoryPage
    {
        private Task InitTask;
        private CancellationTokenSource Cts;
        private BaseDirectoryPageVM _vm;

        public delegate BaseDirectoryPageVM fnMakeDirectoryPageVM();
        fnMakeDirectoryPageVM MakeDirectoryPageVM;

        public DirectoryPage(fnMakeDirectoryPageVM fn)
        {
            //ViewModelWrap<DirectoryViewModel> vm = new ViewModelWrap<DirectoryViewModel>();
            //BindingContext = vm.ViewModel;
            InitializeComponent();
            MakeDirectoryPageVM = fn;// () => { return new FieldsDirVM(); };
        }
        protected override void OnAppearing()
        {
            IsBusy = true;
            if (BindingContext is BaseDirectoryPageVM bindVM)
            {
                _vm = bindVM;
            }
            else
            {
                if (null == _vm && null!=MakeDirectoryPageVM)
                    _vm = MakeDirectoryPageVM();
                BindingContext = _vm;
            }

            if (null != Cts)
            {
                if(Cts.IsCancellationRequested)
                {
                    Cts.Dispose();
                    Cts = new CancellationTokenSource();
                }
            }
            else
                Cts = new CancellationTokenSource();
                
            InitTask = Task.Run(()=>VmInitAsync(Cts.Token));
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
        protected override bool OnBackButtonPressed()
        {
            return (null == _vm) ? false : _vm.OnBackButton();
        }
    }
}