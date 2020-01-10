using Microcharts;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddin2MeasurementDonePage : ContentPage
    {
        private List<Microcharts.Entry> entries = new List<Microcharts.Entry>
        {
            //new Microcharts.Entry(200)
            //{
            //    Color = SKColor.Parse("#FF1493"),
            //    Label = "January",
            //    ValueLabel = "200"
            //},
            //new Microcharts.Entry(400)
            //{
            //    Color = SKColor.Parse("#00BFFF"),
            //    Label = "February",
            //    ValueLabel = "400"
            //},
            //new Microcharts.Entry(-100)
            //{
            //    Color = SKColor.Parse("#00CED1"),
            //    Label = "March",
            //    ValueLabel = "-100"
            //}
        };

        private Ddin2Measurement _measurement;
        public Ddin2MeasurementDonePage(Ddin2Measurement measurement)
        {
            _measurement = measurement;
            var vm = new ViewModel<Ddin2MeasurementDoneViewModel>(measurement);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
            var points = DgmConverter.GetXYs(_measurement.DynGraph.ToList(),
                _measurement.Step,
                _measurement.WeightDiscr); ;
            for (int i = 0; i < points.GetUpperBound(0)/2; i++)
            {
                entries.Add(
                    new Microcharts.Entry((float)points[i, 0])
                    {

                    });
            }

            Chart1.Chart = new LineChart
            {
                Entries = entries
            };
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DataRepository.Instance.SaveDdin2Item(_measurement);
            MessagingCenter
                .Send<Ddin2MeasurementDonePage, Ddin2Measurement>(
                this, "Refresh measurement", _measurement);
        }
    }
}