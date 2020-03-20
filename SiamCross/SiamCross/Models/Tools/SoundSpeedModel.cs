using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class SoundSpeedModel : INotifyPropertyChanged
    {
        private int _code;
        private string _name;
        public int Code
        {
            get => _code;
            set
            {
                _code = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Code)));
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public List<KeyValuePair<float, float>> LevelSpeedTable { get; set; }

        public SoundSpeedModel(int code, string name, List<KeyValuePair<float, float>> table)
        {
            Code = code;
            Name = name;
            LevelSpeedTable = table;
        }

        public override string ToString()
        {
            return $"{Name}: {Code}";
        }
    }
}
