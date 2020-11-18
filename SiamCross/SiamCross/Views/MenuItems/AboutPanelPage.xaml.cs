using SiamCross.Models.Tools;
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
    public partial class AboutPanelPage : ContentPage
    {
        public AboutPanelPage()
        {
            BindingContext = this;
            InitializeComponent();
            //lblVersionNumber.Text = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            //lblBuildNumber.Text = DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();
        }
        public string Version
        {
            get
            {
                return DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            }
        }
        public string Build
        {
            get
            {
                return DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();
            }
        }

    }
}