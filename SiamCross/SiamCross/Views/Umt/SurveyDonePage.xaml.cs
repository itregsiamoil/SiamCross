using SiamCross.ViewModels;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Umt
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SurveyDonePage : BaseContentPage
    {
        public SurveyDonePage()
        {
            InitializeComponent();
        }
        void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            if (BindingContext is SurveyDoneVM vm)
            {
                vm.Render(args);
            }
        }
    }
}