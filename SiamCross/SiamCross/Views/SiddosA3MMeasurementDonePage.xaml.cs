using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
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
        private double[,] _points;

        private SiddosA3MMeasurement _measurement;
        public SiddosA3MMeasurementDonePage(SiddosA3MMeasurement measurement)
        {
            _measurement = measurement;
            var vm = new ViewModel<SiddosA3MMeasurementDoneViewModel>(measurement);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();

            _points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                _measurement.Step,
                _measurement.WeightDiscr);
        }

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
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
            canvas.DrawLine(0, 0, 0, (float)CanvasView.Height, paintAxies);
            canvas.DrawLine(0, (float)CanvasView.Height,
                (float)CanvasView.Width, (float)CanvasView.Height, paintAxies);
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
                skPoints.Add(
                    new SKPoint(
                        (float)(_points[i, 0] * dx),
                        (float)(_points[i, 1] * dy)));
            }
            
            canvas.DrawPoints(SKPointMode.Polygon, skPoints.ToArray(), paint);
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
            base.OnDisappearing();
            DataRepository.Instance.SaveSiddosA3MMeasurement(_measurement);
            MessagingCenter
                .Send<SiddosA3MMeasurementDonePage, SiddosA3MMeasurement>(
                this, "Refresh measurement", _measurement);
        }
    }
}