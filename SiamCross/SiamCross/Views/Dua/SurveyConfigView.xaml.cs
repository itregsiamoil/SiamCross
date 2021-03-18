using SiamCross.Models.Sensors;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Dua
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SurveyConfigView : ContentPage
    {
        ISensor _Sensor;
        public SurveyConfigView(ISensor sensor)
        {
            _Sensor = sensor;
            BindingContext = _Sensor;
            InitializeComponent();
        }
    }
}