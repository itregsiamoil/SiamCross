using SiamCross.Models.Tools;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundSpeedViewPage : ContentPage
    {
        public SoundSpeedViewPage(SoundSpeedModel soundSpeedModel)
        {
            var vm = new ViewModel<SoundSpeedViewViewModel>(soundSpeedModel);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}