using SiamCross.ViewModels;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : BaseContentPage
    {
        public SettingsPanelPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }
    }
}