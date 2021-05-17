using SiamCross.Models;
using SiamCross.Models.Sensors.Umt.Surveys;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.IO;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SurveyDoneVM : BasePageVM
    {
        readonly SurveyDoneModel _Model;
        public PositionVM Position { get; }
        public DeviceInfoVM DeviceInfo { get; }
        public SurveyInfoVM SurveyInfo { get; }
        public DistributionInfoVM MailDistribution { get; }
        public DistributionInfoVM FileDistribution { get; }

        public SurveyDoneVM(SurveyDoneModel data)
        {
            _Model = data;
            Position = new PositionVM(_Model.Position);
            DeviceInfo = new DeviceInfoVM(_Model.DeviceInfo);
            SurveyInfo = new SurveyInfoVM(_Model.SurveyInfo);
            MailDistribution = new DistributionInfoVM(_Model.MailDistribution);
            FileDistribution = new DistributionInfoVM(_Model.FileDistribution);

            PrepareGraph();
        }
        public string SurveyName { get; private set; }
        public uint MeasurementsCount { get; private set; }
        public uint Interval { get; private set; }


        float MinPress = float.MaxValue;
        float MaxPress = float.MinValue;

        float MinTemp = float.MaxValue;
        float MaxTemp = float.MinValue;

        DateTime MinX = DateTime.MaxValue;
        DateTime MaxX = DateTime.MinValue;

        void PrepareGraph()
        {
            if (!_Model.SurveyInfo.DataInt.TryGetValue("umttype", out long kind))
                return;
            if (!_Model.SurveyInfo.DataInt.TryGetValue("PeriodSec", out long periodSec))
                return;
            if (!_Model.SurveyInfo.DataInt.TryGetValue("MeasurementsCount", out long measurementsCount))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MinPressure", out double minPress))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MaxPressure", out double maxPress))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MinIntTemperature", out double minIntTemp))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MaxIntTemperature", out double maxIntTemp))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MinExtTemperature", out double minExtTemp))
                return;
            if (!_Model.SurveyInfo.DataFloat.TryGetValue("MaxExtTemperature", out double maxExtTemp))
                return;

            SurveyName = ((Kind)kind).Title();
            Interval = (uint)periodSec;
            MeasurementsCount = (uint)measurementsCount;

            MinX = _Model.SurveyInfo.BeginTimestamp;
            MaxX = MinX + TimeSpan.FromSeconds(periodSec * measurementsCount);

            MinPress = (float)minPress;
            MaxPress = (float)maxPress;
            MinTemp = (float)minIntTemp;
            MaxTemp = (float)maxIntTemp;

            if (float.MinValue < minExtTemp && minExtTemp < MinTemp)
                MinTemp = (float)minExtTemp;
            if (float.MaxValue > maxExtTemp && maxExtTemp > MaxTemp)
                MaxTemp = (float)maxExtTemp;
        }
        public virtual SKRect DrawAxies(SKCanvas canvas, SKImageInfo info)
        {
            Color txtColor = Color.DarkGray;
            var appRes = Application.Current.Resources;
            if (appRes.TryGetValue("colorText", out object obj_txt_color))
                if (obj_txt_color is Color resTxtColor)
                    txtColor = resTxtColor;

            SKPaint paintAxies = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = txtColor.ToSKColor(),
                StrokeWidth = 0,
            };

            var dens = (float)Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            paintAxies.TextSize = dens * (float)Device.GetNamedSize(NamedSize.Micro, typeof(Label));

            float txtOffset = 2;

            float yLblAxisWidth = (paintAxies.MeasureText("000.00") + txtOffset * 4);
            float yLblAxisHeight = (paintAxies.TextSize + txtOffset * 2) * 2;

            float xLblAxisWidth = (paintAxies.MeasureText("00.00.00") + 4) * 1.2f;
            float xLblAxisHeight = (paintAxies.TextSize + txtOffset * 2) * 2;

            var rect = SKRect.Create(
                yLblAxisWidth
                , yLblAxisHeight / 2
                , info.Width - yLblAxisWidth * 2
                , info.Height - yLblAxisHeight - xLblAxisHeight);

            // draw verical
            int verticalCount = (int)((rect.Right - rect.Left) / (xLblAxisWidth));
            float verticalStep = ((rect.Right - rect.Left) / verticalCount);

            DateTime bottomAxisXStartLabel = MinX;
            TimeSpan bottomAxisXStartStep = TimeSpan.FromSeconds((MaxX - MinX).TotalSeconds / verticalCount);

            for (float i = rect.Left; i < rect.Right + 1; i += verticalStep)
            {
                canvas.DrawLine(i, rect.Top, i, rect.Bottom, paintAxies);
                canvas.DrawText(bottomAxisXStartLabel.ToString("HH:mm:ss"), i - xLblAxisWidth / 2 + txtOffset, rect.Bottom + xLblAxisHeight / 2 - txtOffset, paintAxies);
                canvas.DrawText(bottomAxisXStartLabel.ToString("dd.MM.yy"), i - xLblAxisWidth / 2 + txtOffset, rect.Bottom + xLblAxisHeight - txtOffset, paintAxies);
                bottomAxisXStartLabel += bottomAxisXStartStep;
            }
            // draw horizontal
            int horizontalCount = (int)((rect.Bottom - rect.Top) / (yLblAxisHeight));
            float horizontalStep = ((rect.Bottom - rect.Top) / horizontalCount);

            float leftAxisYStartLabel = MaxPress;
            float leftAxisYStartStep = (MaxPress - MinPress) / horizontalCount;

            float rightAxisYStartLabel = MaxTemp;
            float rightAxisYStartStep = (MaxTemp - MinTemp) / horizontalCount;


            for (float i = rect.Top; i < rect.Bottom + 1; i += horizontalStep)
            {
                canvas.DrawLine(rect.Left, i, rect.Right, i, paintAxies);
                canvas.DrawText(leftAxisYStartLabel.ToString("N3"), 2, i - txtOffset, paintAxies);
                canvas.DrawText(rightAxisYStartLabel.ToString("N2"), rect.Right + 2, i - txtOffset, paintAxies);
                leftAxisYStartLabel -= leftAxisYStartStep;
                rightAxisYStartLabel -= rightAxisYStartStep;
            }
            return rect;
        }

        public virtual void DrawLineСhart(SKCanvas canvas, SKRect rect
            , string dataname, Color color, float minVal, float maxVal)
        {
            if (!_Model.SurveyInfo.DataBlob.TryGetValue(dataname, out string filename))
                return;
            FileStream file = OpenTempFile(filename);
            if (null == file || 4 > file.Length)
                return;

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color.ToSKColor(),
                StrokeWidth = 2,
                LcdRenderText = true,
                IsAntialias = true
            };
            using (file)
            {
                long pointStep;
                if (MeasurementsCount <= rect.Width)
                    pointStep = 1;
                else
                    pointStep = (long)(MeasurementsCount / rect.Width);


                double dx = rect.Width / (file.Length / 4 / pointStep - 0);
                double dy = rect.Height / (maxVal - minVal);
                //SKPoint[] skPoints = new SKPoint[file.Length/4];
                byte[] b = new byte[4];
                float val;
                long i = 0;
                SKPath path = new SKPath();

                file.Read(b, 0, 4);
                val = BitConverter.ToSingle(b, 0);
                path.MoveTo((float)(i * dx) + rect.Left, rect.Top + rect.Height - (float)((val - minVal) * dy));
                i++;

                while (0 < file.Read(b, 0, 4))
                {
                    val = BitConverter.ToSingle(b, 0);
                    path.LineTo((float)(i * dx) + rect.Left, rect.Top + rect.Height - (float)((val - minVal) * dy));
                    i++;
                    file.Seek(4 * (pointStep - 1), SeekOrigin.Current);
                }
                canvas.DrawPath(path, paint);
            }
        }
        public virtual void Render(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            var rect = DrawAxies(canvas, info);

            DrawLineСhart(canvas, rect, "mtpressure", Color.Blue, MinPress, MaxPress);
            DrawLineСhart(canvas, rect, "mttemperature", Color.Orange, MinTemp, MaxTemp);
            DrawLineСhart(canvas, rect, "umttemperatureex", Color.Red, MinTemp, MaxTemp);


        }
        public override void Unsubscribe()
        {
            //    throw new NotImplementedException();
        }
        static FileStream OpenTempFile(string name)
        {
            try
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal), "bin");
                path = Path.Combine(path, name);

                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {

            }
            return null;
        }
    }
}
