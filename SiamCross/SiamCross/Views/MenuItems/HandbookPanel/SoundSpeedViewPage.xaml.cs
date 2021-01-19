using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundSpeedViewPage : ContentPage
    {
        public SoundSpeedViewPage(SoundSpeedModel soundSpeedModel)
        {
            ViewModelWrap<SoundSpeedViewViewModel> vm = new ViewModelWrap<SoundSpeedViewViewModel>(soundSpeedModel);
            BindingContext = vm.ViewModel;

            InitializeComponent();
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert(
                      Resource.Attention,
                      Resource.DeleteQuestion,
                      Resource.YesButton,
                      Resource.NotButton);
            if (result)
            {
                (BindingContext as SoundSpeedViewViewModel).Delete();
            }
        }

        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            try
            {
                SKImageInfo info = args.Info;
                SKSurface surface = args.Surface;
                SKCanvas canvas = surface.Canvas;

                canvas.Clear();

                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.Blue.ToSKColor(),
                    StrokeWidth = 2
                };

                SKPaint paintAxies = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.Black.ToSKColor(),
                    StrokeWidth = 1
                };

                SoundSpeedViewViewModel vm = (SoundSpeedViewViewModel)BindingContext;

                double maxX = vm.GetMaximumX();
                double maxY = vm.GetMaximumY();
                double minX = vm.GetMinimumX();
                double minY = vm.GetMinimumY();


                double dx = CanvasView.Width / (maxX - minX);
                double dy = CanvasView.Height / (maxY - minY);

                List<SKPoint> skPoints = new List<SKPoint>();

                foreach (KeyValuePair<float, float> pair in vm.Points)
                {
                    float y = (float)CanvasView.Height - (float)((pair.Value - minY) * dy);
                    float x = (float)((pair.Key - minX) * dx);
                    skPoints.Add(new SKPoint(x, y));
                }

                canvas.DrawPoints(SKPointMode.Polygon, skPoints.ToArray(), paint);
                canvas.DrawLine(1, 1, 1, (float)CanvasView.Height - 1, paintAxies);
                canvas.DrawLine(1, (float)CanvasView.Height - 1,
                    (float)CanvasView.Width - 1, (float)CanvasView.Height - 1, paintAxies);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanvasView_PaintSurface" + "\n");
                throw;
            }
        }



        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width > height)
            {
                outerStack.Orientation = StackOrientation.Horizontal;
                graphGrid.HeightRequest = 180;
                graphGrid.MinimumHeightRequest = 180;
                graphGrid.WidthRequest = 240;
                graphGrid.MinimumWidthRequest = 240;
            }
            else
            {
                outerStack.Orientation = StackOrientation.Vertical;
                graphGrid.HeightRequest = -1;
                graphGrid.MinimumHeightRequest = -1;
                graphGrid.WidthRequest = -1;
                graphGrid.MinimumWidthRequest = -1;
            }
        }

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
    }
}