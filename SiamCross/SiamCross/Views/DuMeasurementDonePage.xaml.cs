using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
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
    public partial class DuMeasurementDonePage : ContentPage
    {
        private static readonly Logger _logger = 
            AppContainer.Container.Resolve<ILogManager>().GetLog();

        private double[,] _points;

        private readonly DuMeasurement _measurement;

        public DuMeasurementDonePage(DuMeasurement measurement)
        {
            try
            {
                _measurement = measurement;
                var vmWrap = new ViewModelWrap<DuMeasurementDoneViewModel>(measurement);
                this.BindingContext = vmWrap.ViewModel;
                InitializeComponent();
                _points = EchogramConverter.GetPoints(measurement);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DuMeasurementDonePage ctor");
                throw;
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
                    Style = SKPaintStyle.Stroke,
                    Color = Color.Blue.ToSKColor(),
                    StrokeWidth = 1
                };
                SKPaint paintAxies = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.Black.ToSKColor(),
                    StrokeWidth = 1
                };

                DuMeasurementDoneViewModel vm =
                   (DuMeasurementDoneViewModel)BindingContext;

                const float yReserve = 5;
                double maxX = vm.GetMaximumX();
                double maxY = vm.GetMaximumY();
                double minX = vm.GetMinimumX();
                double minY = vm.GetMinimumY();
                double dx = (CanvasView.Width) / (maxX - minX);
                double dy = (CanvasView.Height - yReserve) / (maxY - minY);
                double yOffset = minY;               

                var skPoints = new List<SKPoint>();
                for (int i = 0; i < _points.GetUpperBound(0); i++)
                {
                    float y = (float)CanvasView.Height - 
                        (float)(_points[i, 1] * dy + Math.Abs(yOffset) * dy + yReserve / 2);
                    float x = (float)(_points[i, 0] * dx);

                    skPoints.Add(new SKPoint(x, y));
                }

                List<float> pList = new List<float>();
                for (int i = 0; i < skPoints.Count - 1; i++)
                {
                    canvas.DrawLine(skPoints[i], skPoints[i + 1], paint);
                    if(skPoints[i].Y > CanvasView.Height)
                    {
                        pList.Add(skPoints[i].Y);
                    }
                }

                canvas.DrawLine(1, 1, 1, (float)CanvasView.Height - 1, paintAxies);
                canvas.DrawLine(1, (float)CanvasView.Height - 1,
                    (float)CanvasView.Width - 1, (float)CanvasView.Height - 1, paintAxies);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanvasView_PaintSurface Ddin2MeasurementDonePage");
                throw;
            }
        }

        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                DataRepository.Instance.SaveDuMeasurement(_measurement);
                MessagingCenter
                    .Send(this, "Refresh measurement", _measurement);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OnDisappearing");
                throw;
            }
        }
    }
}