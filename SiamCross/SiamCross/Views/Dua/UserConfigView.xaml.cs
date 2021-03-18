using SiamCross.Models.Sensors;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Dua
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserConfigView : ContentPage
    {
        ISensor _Sensor;
        public UserConfigView(ISensor sensor)
        {
            _Sensor = sensor;
            BindingContext = _Sensor;
            InitializeComponent();
        }
    }
}