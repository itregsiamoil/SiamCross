using SiamCross.Models.Tools;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPanelPage : ContentPage
    {
        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
        public string Build => DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();
        public ICommand CmdShowChanges { get; }
        private int _VersionClickCount = 0;

        public AboutPanelPage()
        {
            CmdShowChanges = new Command(ShowChanges);
            BindingContext = this;
            InitializeComponent();
            //lblVersionNumber.Text = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            //lblBuildNumber.Text = DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();
        }

        private void ShowChanges(object obj)
        {
            _VersionClickCount++;
            if (_VersionClickCount > 5)
            {
                ChangesView.IsVisible = true;
            }
            if (_VersionClickCount > 10)
            {
                ChangesView.IsVisible = false;
                _VersionClickCount = 0;
            }
        }

        public string VersionChanges => GetVersionChanges();
        public string GetVersionChanges()
        {
            //Получаем текущую сборку.
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            string resourceName = "VersionChanges.txt"; // В этом поле указываем имя нашего ресурса.
                                                        //(!) Если ресурс находится в папке, то указываем полный путь к нему, например "content\resource\image.jpg". Но не забываем, что при формировании полного имени ресурса, заменяем знак cлэш '\' на точку '.'
                                                        //Формируем полное имя ресурса.
            string fullResourceName = $"{myAssembly.GetName().Name}.{resourceName.Replace('\\', '.')}";
            bool isExistsResourceName = myAssembly.GetManifestResourceNames()
                .Contains(fullResourceName); //уточняем существует этот ресурс в данной сборке.

            string ret = string.Empty;
            //byte[] result;
            //StringBuilder builder = new StringBuilder();
            //Если ресурс существует, то извлекаем его.
            if (isExistsResourceName)
            {
                Stream stream = myAssembly.GetManifestResourceStream(fullResourceName);
                //result = new byte[stream.Length];
                StreamReader reader = new StreamReader(stream);
                ret = reader.ReadToEnd();
            }
            return ret;
        }

    }
}