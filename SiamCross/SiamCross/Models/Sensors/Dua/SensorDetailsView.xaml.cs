using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Models.Sensors.Dua
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDetailsView : ContentPage
    {
        public SensorDetailsView(ScannedDeviceInfo sensorData)
        {
            InitializeComponent();
        }
    }
}