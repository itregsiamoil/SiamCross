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
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddin2MeasurementDonePage : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private readonly double[,] _points;
        private readonly Ddin2Measurement _measurement;

        public float MinX { get; set; }
        public float MinY { get; set; }
        public float MaxX { get; set; }
        public float MaxY { get; set; }
        public Ddin2MeasurementDonePage(Ddin2Measurement measurement)
        {
            try
            {
                _measurement = measurement;
                ViewModelWrap<Ddin2MeasurementDoneViewModel> vm = new ViewModelWrap<Ddin2MeasurementDoneViewModel>(measurement);
                BindingContext = vm.ViewModel;
                InitializeComponent();
                float minX;
                float minY;
                float maxX;
                float maxY;
                _points = DgmConverter.GetXYs(_measurement.DynGraph.ToList()
                    , _measurement.Step
                    , _measurement.WeightDiscr
                    , _measurement.Period
                    , out minX, out maxX, out minY, out maxY);

                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddin2MeasurementDonePage constructor" + "\n");
                throw;
            }
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            /*
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
            */
        }
        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            try
            {
                SKImageInfo info = args.Info;
                SKSurface surface = args.Surface;
                SKCanvas canvas = surface.Canvas;

                canvas.Clear();

                SKPaint paintFill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.Accent.ToSKColor().WithAlpha(40),
                };

                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color.Accent.ToSKColor(),
                    StrokeWidth = 0,
                    LcdRenderText = true,
                    IsAntialias = true
                };
                SKPaint paintAxies = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color.DarkGray.ToSKColor(),
                    StrokeWidth = 2
                };
                SKPaint paintText = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.DarkGray.ToSKColor(),
                    StrokeWidth = 1,
                    LcdRenderText = true,
                    IsAntialias = true
                };

                //float canvHeight1 = (float)CanvasView.Height;
                //float canvWidth1 = (float)CanvasView.Width;
                float canvHeight = args.Info.Height;
                float canvWidth = args.Info.Width;
                //canvas.DrawRect(0, 0, canvWidth, canvHeight, paintAxies);
                //canvas.DrawLine(0, 0, 0, canvHeight, paintAxies);
                //canvas.DrawLine(0, canvHeight, canvWidth, canvHeight, paintAxies);

                //float textWidth = paintText.MeasureText("0.00");
                //float textHeight = paintText.TextSize;
                //paintText.TextSize = 1.0f * info.Width * paintText.TextSize / textWidth;
                //canvas.DrawText(_measurement.MaxWeight.ToString("N2"), new SKPoint(0, textHeight), paintText);
                //canvas.DrawText(_measurement.MinWeight.ToString("N2"), new SKPoint(0, canvHeight- textHeight), paintText);

                float dgm_w = (float)(MaxX - MinX);
                float dgm_h = (float)(MaxY - MinY);

                float scale_x = (float)((canvWidth) / (MaxX - MinX));
                float scale_y = (float)((canvHeight) / (MaxY - MinY));
                float offset_x = (float)MinX;
                float offset_y = (float)MinY;

                int maxpoints = (_measurement.Period > _points.GetLength(0)) ?
                    _points.GetLength(0) : _measurement.Period;



                SKPoint[] skPoints = new SKPoint[maxpoints];
                for (int i = 0; i < maxpoints; i++)
                {
                    //skPoints[i].Y = (float)(CanvasView.Height - (_points[i, 1]  - offset_y)* scale_y);
                    //skPoints[i].X = (float)((_points[i, 0]  - offset_x)* scale_x);
                    skPoints[i].Y = (float)(_points[i, 1]);
                    skPoints[i].X = (float)(_points[i, 0]);

                }
                //canvas.DrawPoints(SKPointMode.Polygon, skPoints, paint);
                SKPath path2 = new SKPath { FillType = SKPathFillType.Winding };
                path2.AddPoly(skPoints, true);

                float sx = (float)scale_x;
                float sy = (float)scale_y;
                float px = 0;
                float py = 0;
                canvas.Scale(sx, sy, px, py);
                canvas.Scale(1.0f, -1.0f, dgm_w / 2, dgm_h / 2);
                canvas.Translate(-offset_x, -offset_y);
                canvas.DrawPath(path2, paintFill);
                canvas.DrawPath(path2, paint);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanvasView_PaintSurface Ddin2MeasurementDonePage" + "\n");
                throw;
            }
        }
        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                DataRepository.Instance.SaveDdin2Measurement(_measurement);
                MessagingCenter
                    .Send<Ddin2MeasurementDonePage, Ddin2Measurement>(
                    this, "Refresh measurement", _measurement);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OnDisappearing" + "\n");
                throw;
            }
        }
    }
}