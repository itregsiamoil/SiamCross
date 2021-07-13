using SiamCross.Models.Tools;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Dmg
{
    public class SliderFloat : Slider
    {
        public SliderFloat()
            :base(float.MinValue, float.MaxValue, (double)0.0f)
        {
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DynamogrammSurveyCfgPage
    {
        public DynamogrammSurveyCfgPage()
        {
            InitializeComponent();
        }

    }
}