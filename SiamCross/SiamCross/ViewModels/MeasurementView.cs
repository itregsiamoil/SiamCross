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

        bool mSending = false;
        public bool Sending 
        { 
            get=> mSending; 
            set
            {
                mSending = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Sending)));
            }
        }

        DateTime mZeroTs = new DateTime(0);
        DateTime mLastSentTimestamp=new DateTime(0);
        public void SetLastSentTimestamp( DateTime ts)
        {
            mLastSentTimestamp = ts;
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(LastSentTimestamp)));
        }
        public string LastSentTimestamp
        {
            get => (mLastSentTimestamp == mZeroTs)? "" : mLastSentTimestamp.ToString();
        }

        string mLastSentRecipient;
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

        bool mIsSelected = false;
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
