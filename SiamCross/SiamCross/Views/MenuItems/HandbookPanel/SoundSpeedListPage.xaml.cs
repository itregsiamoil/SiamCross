using SiamCross.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    public partial class SoundSpeedListPage : ContentPage
    {
        Task InitTask;
        CancellationTokenSource Cts;
        public SoundSpeedListPage()
        {
            InitializeComponent();
            BindingContext = new SoundSpeedListVM();
        }
        /*
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!(BindingContext is SoundSpeedListVM pvm))
            {
                pvm = new SoundSpeedListVM();
                BindingContext = pvm;
            }

            if (null == Cts || Cts.IsCancellationRequested)
                Cts = new CancellationTokenSource();
            InitTask = Task.Run(async () => await pvm.InitAsync(Cts.Token).ConfigureAwait(false));
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is BasePageVM pvm)
            {
                Task.Run(async () =>
                {
                    if (null!= InitTask && !InitTask.IsCompleted)
                    {
                        Cts?.Cancel();
                        await InitTask;
                    }
                    pvm.Unsubscribe();
                });
            }
            //BindingContext = null;
        }
        */
    }
}