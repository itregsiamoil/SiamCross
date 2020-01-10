using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : ContentPage
    {
        private List<Microcharts.Entry> entries = new List<Microcharts.Entry>
        {
            new Microcharts.Entry(200)
            {
                Color = SKColor.Parse("#FF1493"),
                Label = "January",
                ValueLabel = "200"
            },
            new Microcharts.Entry(400)
            {
                Color = SKColor.Parse("#00BFFF"),
                Label = "February",
                ValueLabel = "400"  
            },
            new Microcharts.Entry(-100)
            {
                Color = SKColor.Parse("#00CED1"),
                Label = "March",
                ValueLabel = "-100"
            }
        }; 
        public SettingsPanelPage()
        {
            InitializeComponent();

            Chart1.Chart = new LineChart
            { 
                Entries = entries 
            };
        }
    }
}