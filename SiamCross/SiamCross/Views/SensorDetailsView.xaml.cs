using SiamCross.Models.Sensors;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDetailsView : ContentPage
    {
        ISensor _Sensor;
        public SensorDetailsView(ISensor sensor)
        {
            _Sensor = sensor;
            BindingContext = _Sensor;
            var mes = Services.DataRepository.Instance
                .GetDdin2Measurements().Select(m => m);
            if (mes.Any())
            {
                var _measurement = mes.Last();
                sensor.Position.Field = _measurement.Field;
                sensor.Position.Well = _measurement.Well;
                sensor.Position.Bush = _measurement.Bush;
                sensor.Position.Shop = _measurement.Shop;
            }


            InitializeComponent();
        }

    }
}