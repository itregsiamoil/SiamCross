using SiamCross.Models.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SiamCross.Models
{
    public class SensorData : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        public string Type { get; private set; }

        public Dictionary<string, string> Parameters { get; }

        public ObservableCollection<string> Params { get; }

        public void SetProperty(string name, string value)
        {
            if (Parameters.ContainsKey(name))
            {
                Parameters[name] = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
            }
        }

        private string _status;

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }

        public int Id { get; private set; }

        public SensorData(int id, string name, string type, string status)
        {
            Id = id;
            Name = name;
            Type = type;
            Status = status;
            Parameters = Constants.GetQuickReportDictionary();
            Params = new ObservableCollection<string>
            {
                "gsfg",
                "sfgs",
                "sfgs",
                "sfgs"
            };
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
