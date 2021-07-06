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
        protected override bool OnBackButtonPressed()
        {
            return (null == _vm) ? false : _vm.OnBackButton();
        }
    }
}