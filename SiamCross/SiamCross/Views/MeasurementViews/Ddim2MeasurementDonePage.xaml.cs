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
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddim2MeasurementDonePage : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private double[,] _points;
        private ViewModelWrap<Ddim2MeasurementDoneViewModel> _vm;
        private Ddim2Measurement _measurement;
        public Ddim2MeasurementDonePage(Ddim2Measurement measurement)
        {
            try
            {
                _measurement = measurement;
                _vm = new ViewModelWrap<Ddim2MeasurementDoneViewModel>(measurement);
                this.BindingContext = _vm.ViewModel;
                InitializeComponent();

                _points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                    _measurement.Step,
                    _measurement.WeightDiscr);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Ddim2MeasurementDonePage constructor" + "\n");
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

                double maxX = GetMaximumX();
                double maxY = GetMaximumY();
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
            catch (System.Exception ex)
            {
                _logger.Error(ex, "CanvasView_PaintSurface" + "\n");
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
                DataRepository.Instance.SaveDdim2Measurement(_measurement);
                MessagingCenter
                    .Send<Ddim2MeasurementDonePage, Ddim2Measurement>(
                    this, "Refresh measurement", _measurement);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "OnDisappearing" + "\n");
                throw;
            }
        }

        private async void ToolbarItem_Clicked(object sender, System.EventArgs e)
        {
            //_vm.ViewModel.ShareCommandHandler();
            System.Diagnostics.Debug.WriteLine("1");
            try
            {
                var xmlCreator = new XmlCreator();
                System.Diagnostics.Debug.WriteLine("2");
                var xmlSaver = DependencyService.Get<IXmlSaver>();
                System.Diagnostics.Debug.WriteLine("3");
                var name = CreateName(_measurement.Name, _measurement.DateTime);
                System.Diagnostics.Debug.WriteLine("4");
                xmlSaver.SaveXml(name, xmlCreator.CreateDdim2Xml(_measurement));
                System.Diagnostics.Debug.WriteLine("5");
                string filepath = xmlSaver.GetFilepath(name);
                System.Diagnostics.Debug.WriteLine("6");
                var title = Resource.ShareMeasurement;
                System.Diagnostics.Debug.WriteLine("7");

                var file = new ShareFile(filepath);
                System.Diagnostics.Debug.WriteLine("8");

                var request = new ShareFileRequest();
                //var request = new ShareFileRequest
                //{
                //    Title = title,
                //    File = file
                //};

                System.Diagnostics.Debug.WriteLine("9");
                //await Share.RequestAsync(request);
                System.Diagnostics.Debug.WriteLine("810");
            }
            catch (Exception ex)
            {
               _logger.Error(ex, "ShareCommandHandler" + "\n");
                throw;
            }

        }

        private string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{_timeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }

        private DateTimeConverter _timeConverter = new DateTimeConverter();
    }
}