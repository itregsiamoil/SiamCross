using SiamCross.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiamCross.ViewModels
{
    public class MeasurementView : INotifyPropertyChanged
    {
        private readonly SurveyInfo _Survey = new SurveyInfo();

        public event PropertyChangedEventHandler PropertyChanged;
        public void ChangeNotify([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public int Id
        { get => _Survey.Id; set { _Survey.Id = value; ChangeNotify(); } }
        public string Name
        { get => _Survey.Device.Name; set { _Survey.Device.Name = value; ChangeNotify(); } }
        public string Field
        { get => _Survey.Position.Field; set { _Survey.Position.Field = value; ChangeNotify(); } }
        public string Comments
        { get => _Survey.Measure.Comment; set { _Survey.Measure.Comment = value; ChangeNotify(); } }
        public DateTime Date
        { get => _Survey.Measure.EndTimestamp; set { _Survey.Measure.EndTimestamp = value; ChangeNotify(); } }
        public uint MeasureKind
        { get => _Survey.Measure.Kind; set { _Survey.Measure.Kind = value; ChangeNotify(); } }
        public string MeasureKindName
        {
            get
            {
                if (MeasurementIndex.Instance.TryGetName(_Survey.Measure.Kind, out string name))
                    return name;
                return string.Empty;
            }
            set
            {
                if (uint.TryParse(value, out uint idx))
                {
                    if (MeasurementIndex.Instance.TryGetName(idx, out string name))
                    {
                        _Survey.Measure.Kind = idx;
                        ChangeNotify();
                    }
                    return;
                }
                if (MeasurementIndex.Instance.TryGetId(value, out uint idxx))
                {
                    _Survey.Measure.Kind = idxx;
                    ChangeNotify();
                }
            }
        }

        public string LastSentTimestamp
        {
            get => DateTime.MinValue == _Survey.MailDistribution.Timestamp ? string.Empty : _Survey.MailDistribution.Timestamp.ToString();
            set { DateTime.TryParse(value, out _Survey.MailDistribution.Timestamp); ChangeNotify(); }
        }
        public string LastSentRecipient
        {
            get => _Survey.MailDistribution.Destination;
            set { _Survey.MailDistribution.Destination = value; ChangeNotify(); }
        }

        public string LastSaveTimestamp
        {
            get => DateTime.MinValue == _Survey.FileDistribution.Timestamp ? string.Empty : _Survey.FileDistribution.Timestamp.ToString();
            set { DateTime.TryParse(value, out _Survey.FileDistribution.Timestamp); ChangeNotify(); }
        }
        public string LastSaveFolder
        {
            get => _Survey.FileDistribution.Destination;
            set { _Survey.FileDistribution.Destination = value; ChangeNotify(); }
        }

        // view info 
        public bool Sending
        { get => _Sending; set { _Sending = value; ChangeNotify(); } }
        private bool _Sending = false;
        public bool Saving
        { get => _Saving; set { _Saving = value; ChangeNotify(); } }
        private bool _Saving;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected == value)
                    return;
                _IsSelected = value;
                ChangeNotify();
            }
        }
        private bool _IsSelected = false;

    }
}
