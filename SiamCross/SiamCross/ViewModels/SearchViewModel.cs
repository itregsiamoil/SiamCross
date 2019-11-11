using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SiamCross.Models.Scanners;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models;

namespace SiamCross.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        private MainPageModel _mainPageModel = new MainPageModel();
        private IBluetoothScanner _bluetoothScaner;
        public ObservableCollection<string> ListViewBtItems { get; set; }
        public string Count
        {
            get
            {
                return ListViewBtItems.Count.ToString();
            }
        }

        private void OnDeviceDetected(ScannedDeviceInfo deviceInfo)
        {
            Add(deviceInfo.Name, deviceInfo.BluetoothArgs);
        }

        private void Add(string name, object bluetoothArgs)
        {
            if (name == null || bluetoothArgs == null)
            {
                return;
            }

            if (!_mainPageModel.DeviceDict.ContainsKey(name) && name != "")
            {
                _mainPageModel.DeviceDict.Add(name, bluetoothArgs);
                Device.BeginInvokeOnMainThread(() =>
                {
                    ListViewBtItems.Add(name);
                });
                NotifyPropertyChanged(nameof(ListViewBtItems));
                NotifyPropertyChanged(nameof(Count));
            }
        }

        public object GetValue(string name)
        {
            return _mainPageModel.DeviceDict[name];
        }

        public SearchViewModel()
        {
            _bluetoothScaner = DependencyService.Get<IBluetoothScanner>();
            _bluetoothScaner.Received += OnDeviceDetected;
            ListViewBtItems = new ObservableCollection<string>();
            _bluetoothScaner.Start();
        }
    }
}
