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
        void DetectMinMaxPressureGraph()
        {
            if (!_Model.SurveyInfo.DataBlob.TryGetValue("mtpressure", out string filename))
                return;
            using (FileStream file = OpenTempFile(filename))
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
            SKPaint paintAxies = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color.DarkGray.ToSKColor(),
                StrokeWidth = 0,
            };

            paintAxies.TextSize = (float)Device.GetNamedSize(NamedSize.Small, typeof(Label));

            float axisLabelWidth = paintAxies.MeasureText("0000") + 4;
            float axisLabelHeight = paintAxies.TextSize + 4;

            var rect = SKRect.Create(
                axisLabelWidth
                , axisLabelHeight
                , info.Width - axisLabelWidth*2
                , info.Height - axisLabelHeight*2);
            //float leftOffset = axisLabelWidth;
            //float topOffset = axisLabelHeight;
            //float rightOffset = info.Width - axisLabelWidth;
            //float bottomOffset = info.Height - axisLabelHeight;

            // draw verical
            int verticalCount = (int)((rect.Right - rect.Left) / (axisLabelWidth * 2));
            float verticalStep = ((rect.Right - rect.Left) / verticalCount);
            for (float i = rect.Left; i < rect.Right + 1; i += verticalStep)
            {
                canvas.DrawLine(i, rect.Top, i, rect.Bottom, paintAxies);
                canvas.DrawText("0000", i - axisLabelWidth / 2 + 2, rect.Bottom + axisLabelHeight - 2, paintAxies);
            }
            // draw horizontal
            int horizontalCount = (int)((rect.Bottom - rect.Top) / (axisLabelHeight * 4));
            float horizontalStep = ((rect.Bottom - rect.Top) / horizontalCount);

            float leftAxisYStartLabel = MaxPress;
            float leftAxisYStartStep = (MaxPress - MinPress)/ horizontalCount;

            for (float i = rect.Top; i < rect.Bottom + 1; i += horizontalStep)
            {
                canvas.DrawLine(rect.Left, i, rect.Right, i, paintAxies);
                canvas.DrawText(leftAxisYStartLabel.ToString("N2"), 2, i - 2, paintAxies);
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
                StrokeWidth = 0,
                LcdRenderText = true,
                IsAntialias = true
            };

            using (FileStream file = OpenTempFile(filename))
            {
                if(file.Length>=4)
                {
                    float dx = rect.Width / (file.Length / 4 - 0);
                    float dy = rect.Height / (MaxPress - MinPress);
                    //SKPoint[] skPoints = new SKPoint[file.Length/4];
                    byte[] b = new byte[4];
                    float val;
                    uint i = 0;
                    SKPath path = new SKPath();

                    file.Read(b, 0, 4);
                    val = BitConverter.ToSingle(b, 0);
                    path.MoveTo(i * dx + rect.Left, rect.Height - val * dy + rect.Top);
                    i++;

                    while (0 < file.Read(b, 0, 4))
                    {
                        val = BitConverter.ToSingle(b, 0);
                        //skPoints[i].Y = val*dy;
                        //skPoints[i].X = i*dx;
                        path.LineTo(i * dx + rect.Left, rect.Height-val * dy + rect.Top);
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
