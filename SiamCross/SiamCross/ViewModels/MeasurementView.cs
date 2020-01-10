using System;
using System.ComponentModel;

namespace SiamCross.ViewModels
{
    public class MeasurementView : INotifyPropertyChanged
    {
        private string _field;
        private string _comments;

        public int Id { get; set; }

        public string Name { get; set; }
        public string Field
        {
            get => _field;
            set
            {
                _field = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Field)));
            }
        }
        public DateTime Date { get; set; }
        public string MeasurementType { get; set; }
        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Comments)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
