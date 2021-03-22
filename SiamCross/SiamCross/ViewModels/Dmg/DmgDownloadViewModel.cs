using SiamCross.Models.Sensors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.Dmg
{
    public class DmgDownloadViewModel: BaseVM
    {
        ISensor _Sensor;
        public ISensor Sensor
        {
            get => _Sensor;
            set
            {
                _Sensor = value;
                ChangeNotify();
            }
        }



        public ICommand StartDownloadCommand { get; set; }

        public DmgDownloadViewModel()
        {
            StartDownloadCommand = new AsyncCommand(StartDownload
                , (Func<object, bool>)null, null, false, false);

        }

        private async Task StartDownload()
        {
            //_Sensor.Downloader
            //return await Task.CompletedTask;
        }
    }
}
