using SiamCross.Models.Sensors;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Dua
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FactoryConfigView : ContentPage
    {
        ISensor _Sensor;
        public FactoryConfigView(ISensor sensor)
        {
            _Sensor = sensor;
            BindingContext = _Sensor;
            InitializeComponent();
        }
    }
}