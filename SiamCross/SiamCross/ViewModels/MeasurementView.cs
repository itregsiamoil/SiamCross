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

        private bool _Sending = false;
        public bool Sending
        {
            get => _Sending;
            set
            {
                _Sending = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Sending)));
            }
        }

        private string _LastSentTimestamp = "";
        public string LastSentTimestamp
        {
            get => _LastSentTimestamp;
            set
            {
                _LastSentTimestamp = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(LastSentTimestamp)));
            }
        }
        private string mLastSentRecipient;
        public string LastSentRecipient
        {
            get => mLastSentRecipient;
            set
            {
                mLastSentRecipient = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(LastSentRecipient)));
            }
        }

        private bool _IsSelected = false;
        public void SetSelected(bool sel)
        {
            _IsSelected = sel;
        }
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected == value)
                    return;
                _IsSelected = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _Saving;
        private string _LastSaveTimestamp = "";
        private string _LastSaveFolder;
        public bool Saving
        {
            get => _Saving;
            set
            {
                _Saving = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Saving)));
            }
        }
        public string LastSaveTimestamp
        {
            get => _LastSaveTimestamp;
            set
            {
                _LastSaveTimestamp = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(LastSaveTimestamp)));
            }
        }
        public string LastSaveFolder
        {
            get => _LastSaveFolder;
            set
            {
                _LastSaveFolder = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(LastSaveFolder)));
            }
        }
    }
}
