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
    public partial class Ddin2MeasurementDonePage : ContentPage
    {
        private double[,] _points;

        private Ddin2Measurement _measurement;
        public Ddin2MeasurementDonePage(Ddin2Measurement measurement)
        {
            _measurement = measurement;
            var vm = new ViewModel<Ddin2MeasurementDoneViewModel>(measurement);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();

            _points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                _measurement.Step,
                _measurement.WeightDiscr);
            //var points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
            //    _measurement.Step,
            //    _measurement.WeightDiscr); ;
            //for (int i = 0; i < points.GetUpperBound(0)/3; i++)
            //{
            //    //if (i % 10 != 0) continue;
            //    //else 
            //    entries.Add(
            //        new Microcharts.Entry((float)points[i, 1])
            //        {
            //            //Label = points[i, 0].ToString()
            //        });
            //}

            //Chart1.Chart = new LineChart
            //{
            //    Entries = entries,
            //};
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
                StrokeWidth = 2,
                IsAntialias = true
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
            double dx = (CanvasView.Width) / maxX;
            double dy = (CanvasView.Height) / maxY;

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
            DataRepository.Instance.SaveDdin2Measurement(_measurement);
            MessagingCenter
                .Send<Ddin2MeasurementDonePage, Ddin2Measurement>(
                this, "Refresh measurement", _measurement);
        }
    }
}