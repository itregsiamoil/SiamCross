using SiamCross.Models.Tools;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
        public string Build => DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();

        public string VersionChanges
        {
            get
            {
                //Получаем текущую сборку.
                Assembly myAssembly = Assembly.GetExecutingAssembly();
                string resourceName = "VersionChanges.txt"; // В этом поле указываем имя нашего ресурса.
                                                            //(!) Если ресурс находится в папке, то указываем полный путь к нему, например "content\resource\image.jpg". Но не забываем, что при формировании полного имени ресурса, заменяем знак cлэш '\' на точку '.'
                                                            //Формируем полное имя ресурса.
                string fullResourceName = $"{myAssembly.GetName().Name}.{resourceName.Replace('\\', '.')}";
                bool isExistsResourceName = myAssembly.GetManifestResourceNames()
                    .Contains(fullResourceName); //уточняем существует этот ресурс в данной сборке.


                string ret = "";
                byte[] result;
                StringBuilder builder = new StringBuilder();
                //Если ресурс существует, то извлекаем его.
                if (isExistsResourceName)
                {
                    Stream stream = myAssembly.GetManifestResourceStream(fullResourceName);
                    result = new byte[stream.Length];
                    StreamReader reader = new StreamReader(stream);
                    ret = reader.ReadToEnd();
                }
                return ret;
            }
        }

    }
}