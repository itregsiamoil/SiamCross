using SiamCross.Models;
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

            DetectMinMaxPressureGraph();
        }

        float MinPress = float.MaxValue;
        float MaxPress = float.MinValue;

        DateTime MinX = DateTime.MaxValue;
        DateTime MaxX = DateTime.MinValue;

        void DetectMinMaxPressureGraph()
        {
            if (!_Model.SurveyInfo.DataBlob.TryGetValue("mtpressure", out string filename))
                return;
            if (!_Model.SurveyInfo.DataInt.TryGetValue("PeriodSec", out long periodSec))
                return;

            FileStream file = OpenTempFile(filename);
            if(null== file)
                return;

            MinX = _Model.SurveyInfo.BeginTimestamp;
            MaxX = MinX + TimeSpan.FromSeconds(periodSec);

            using (file)
            {
                byte[] b = new byte[4];
                while (0<file.Read(b,0, 4))
                {
                    float val = BitConverter.ToSingle(b, 0);
                    if (val < MinPress)
                        MinPress = val;
                    if (val > MaxPress)
                        MaxPress = val;
                }
            }
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

            float yLblAxisWidth = (paintAxies.MeasureText("0000") + txtOffset*2);
            float yLblAxisHeight = (paintAxies.TextSize + txtOffset*2) * 2;

            float xLblAxisWidth = (paintAxies.MeasureText("00.00.00") + 4)*1.2f;
            float xLblAxisHeight = (paintAxies.TextSize + txtOffset*2) *2;

            var rect = SKRect.Create(
                yLblAxisWidth
                , yLblAxisHeight/2
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
            float leftAxisYStartStep = (MaxPress - MinPress)/ horizontalCount;

            for (float i = rect.Top; i < rect.Bottom + 1; i += horizontalStep)
            {
                canvas.DrawLine(rect.Left, i, rect.Right, i, paintAxies);
                canvas.DrawText(leftAxisYStartLabel.ToString("N2"), 2, i - txtOffset, paintAxies);
                leftAxisYStartLabel -= leftAxisYStartStep;
            }
            return rect;
        }

        public virtual void DrawPressure(SKCanvas canvas, SKRect rect)
        {
            if (!_Model.SurveyInfo.DataBlob.TryGetValue("mtpressure", out string filename))
                return;

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Accent.ToSKColor(),
                StrokeWidth = 1,
                LcdRenderText = true,
                IsAntialias = true
            };
            FileStream file = OpenTempFile(filename);
            if(null== file)
                return;
            using (file)
            {
                if(file.Length>=4)
                {
                    double dx = rect.Width / (file.Length / 4 - 0);
                    double dy = rect.Height / (MaxPress - MinPress);
                    //SKPoint[] skPoints = new SKPoint[file.Length/4];
                    byte[] b = new byte[4];
                    float val;
                    uint i = 0;
                    SKPath path = new SKPath();

                    file.Read(b, 0, 4);
                    val = BitConverter.ToSingle(b, 0) ;
                    path.MoveTo((float)(i * dx) + rect.Left, rect.Top + rect.Height - (float)(  (val- MinPress) * dy));
                    i++;

                    while (0 < file.Read(b, 0, 4))
                    {
                        val = BitConverter.ToSingle(b, 0);
                        //skPoints[i].Y = val*dy;
                        //skPoints[i].X = i*dx;
                        path.LineTo((float)(i * dx) + rect.Left, rect.Top + rect.Height - (float)((val - MinPress) * dy));
                        i++;
                    }
                    canvas.DrawPath(path, paint);                
                }
            }
        }

        public virtual void Render(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            var rect = DrawAxies(canvas, info);

            DrawPressure(canvas, rect);


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
