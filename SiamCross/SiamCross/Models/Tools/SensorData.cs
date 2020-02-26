using System;
using System.ComponentModel;

namespace SiamCross.Models
{
    public class SensorData : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        public string Type { get; private set; }

        public string Firmware
        {
            get => _firmware;
            set
            {
                _firmware = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Firmware)));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _firmware;

        private string _status;

        public Guid Id { get; private set; }

        public SensorData(Guid id, string name, string type, string status)
        {
            Id = id;
            Name = name;
            Type = type;
            Status = status;
            Firmware = "";
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
