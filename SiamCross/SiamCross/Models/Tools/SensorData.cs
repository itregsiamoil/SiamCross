using System;
using System.ComponentModel;
using Xamarin.Essentials;

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

        public string Battery
        {
            get => _battery;
            set
            {
                _battery = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Battery)));
            }
        }

        public string Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
            }
        }

        public string RadioFirmware
        {
            get => _radiofirmware;
            set
            {
                _radiofirmware = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RadioFirmware)));
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

        private string _firmware="0";
        private string _battery="0";
        private string _temperature="0";
        private string _radiofirmware="0.0.0";
        

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
