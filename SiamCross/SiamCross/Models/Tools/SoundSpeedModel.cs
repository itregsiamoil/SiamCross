using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class SoundSpeedModel : INotifyPropertyChanged
    {
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

        public Dictionary<float, float> LevelSpeedTable { get; set; }

        private int _code;
        private string _name;

        public SoundSpeedModel(int code, string name, Dictionary<float, float> table)
        {
            Code = code;
            Name = name;
            LevelSpeedTable = table;
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
