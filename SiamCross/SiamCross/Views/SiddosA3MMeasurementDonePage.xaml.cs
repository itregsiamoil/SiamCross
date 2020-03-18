using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using SiamCross.Views.MenuItems;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SiddosA3MMeasurementDonePage : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private double[,] _points;

        private SiddosA3MMeasurement _measurement;
        public SiddosA3MMeasurementDonePage(SiddosA3MMeasurement measurement)
        {
            try
            {
                _measurement = measurement;
                var vm = new ViewModelWrap<SiddosA3MMeasurementDoneViewModel>(measurement);
                this.BindingContext = vm.ViewModel;
                InitializeComponent();

                _points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                    _measurement.Step,
                    _measurement.WeightDiscr);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SiddosA3MMeasurementDonePage constructor");
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

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
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
                //canvas.DrawLine(0, 0, 0, (float)CanvasView.Height, paintAxies);
                //canvas.DrawLine(0, (float)CanvasView.Height,
                //    (float)CanvasView.Width, (float)CanvasView.Height, paintAxies);
                //canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);

                double maxX = GetMaximumX();
                double maxY = GetMaximumY();
                //maxX = maxX < 1 ? maxX * 0.1 : maxX * 10;
                //maxY = maxY < 1 ? maxY * 0.1 : maxY * 10;
                double dx = CanvasView.Width / maxX;
                double dy = CanvasView.Height / maxY;

                var skPoints = new List<SKPoint>();
                for (int i = 0; i < _points.GetUpperBound(0); i++)
                {
                    float y = (float)CanvasView.Height - (float)(_points[i, 1] * dy);
                    float x = (float)(_points[i, 0] * dx);
                    skPoints.Add(new SKPoint(x, y));
                }
                canvas.DrawPoints(SKPointMode.Polygon, skPoints.ToArray(), paint);

                canvas.DrawLine(1, 1, 1, (float)CanvasView.Height - 1, paintAxies);
                canvas.DrawLine(1, (float)CanvasView.Height - 1,
                    (float)CanvasView.Width - 1, (float)CanvasView.Height - 1, paintAxies);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanvasView_PaintSurface");
                throw;
            }
        }

        private double GetMaximumX()
        {
            double max = -43;
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 0] > max)
                {
                    max = _points[i, 0];
                }
            }
            return max;
        }

        private double GetMaximumY()
        {
            double max = -43;
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 1] > max)
                {
                    max = _points[i, 1];
                }
            }
            return max;
        }

        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                DataRepository.Instance.SaveSiddosA3MMeasurement(_measurement);
                MessagingCenter
                    .Send<SiddosA3MMeasurementDonePage, SiddosA3MMeasurement>(
                    this, "Refresh measurement", _measurement);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OnDisappearing");
                throw;
            }
        }
    }
}