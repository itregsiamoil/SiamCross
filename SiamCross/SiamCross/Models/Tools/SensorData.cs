using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace SiamCross.Models
{
    [Preserve(AllMembers = true)]
    public class SensorData : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        public string Type { get; private set; }

        private string _status;

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public int Id { get; private set; }

        public SensorData(int id, string name, string type, string status)
        {
            Id = id;
            Name = name;
            Type = type;
            Status = status;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
