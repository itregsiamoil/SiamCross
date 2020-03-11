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
    public partial class DuMeasurementPage : ContentPage
    {
        public DuMeasurementPage()
        {
            InitializeComponent();
        }

        private void AmplificationCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            
        }
        
        private void InletCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            
        }
    }
}